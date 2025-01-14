using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebProductProject.Models;
using System.Text;
using Newtonsoft.Json;

namespace WebProductProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult PlaceOrder()
        {
            // Retrieve product data from query parameters
            string productName = Request.Query["productName"];
            string productDesc = Request.Query["productDesc"];
            string productPrice = Request.Query["productPrice"];
            string productId = Request.Query["productId"];

            _logger.LogInformation($"Received parameters - ProductName: {productName}, ProductDesc: {productDesc}, ProductPrice: {productPrice}, ProductId: {productId}");

            // Check if the values are provided, and handle cases where they may be missing or invalid
            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productDesc) || string.IsNullOrEmpty(productPrice) || string.IsNullOrEmpty(productId))
            {
                _logger.LogError("Error in PlaceOrder action: Missing or invalid product parameters.");

                // Redirect to error if data is missing
                return RedirectToAction("Error", "Home");
            }

            // Try parsing the price and ItemId
            decimal price;
            int itemId;
            if (!decimal.TryParse(productPrice, out price) || !int.TryParse(productId, out itemId))
            {
                _logger.LogError("Error in PlaceOrder action: Failed to parse product price or ID.");

                // Redirect to error if parsing fails
                return RedirectToAction("Error", "Home");
            }

            // Create a model to pass to the view
            var model = new ItemDto
            {
                ItemId = itemId, // Set ItemId
                Name = productName,
                Desc = productDesc,
                Price = price
            };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> SubmitOrder(int itemId, string username, int quantity, decimal productPrice)
        {
            // Validate inputs - Ensure non-empty and positive values
            if (itemId <= 0 || string.IsNullOrEmpty(username) || quantity <= 0 || productPrice <= 0)
            {
                _logger.LogError("Error in SubmitOrder action: Missing or invalid order parameters.");
                return RedirectToAction("Error", "Home");
            }

            // Calculate total price
            decimal totalPrice = productPrice * quantity;

            // Create the order data to send in the API request
            var order = new OrderDto
            {
                ItemId = itemId,
                Quantity = quantity,
                TotalPrice = totalPrice,
                Username = username
            };

            var jsonContent = JsonConvert.SerializeObject(order);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // Retrieve token from cookies
                var token = Request.Cookies["token"];
                if (!string.IsNullOrEmpty(token))
                {
                    // Add Authorization header if token exists
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _logger.LogError("Token is missing in the cookies.");
                    return RedirectToAction("Error", "Home");
                }

                var response = await httpClient.PostAsync("https://localhost:7065/api/items/order", content);

                if (response.IsSuccessStatusCode)
                {


                    return View("OrderConfirmation");
                }
                else
                {
                    // Log the error with more details
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error submitting order: {response.ReasonPhrase} - {errorResponse}");
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting order: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> GetAllOrders()
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // Retrieve token from cookies
                var token = Request.Cookies["token"];
                if (!string.IsNullOrEmpty(token))
                {
                    // Add Authorization header if token exists
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _logger.LogError("Token is missing in the cookies.");
                    return RedirectToAction("Error", "Home");
                }

                // Call the API to get all orders
                var response = await httpClient.GetAsync("https://localhost:7065/api/items/orders");

                if (response.IsSuccessStatusCode)
                {
                    var ordersData = await response.Content.ReadAsStringAsync();

                    // You can use a library like EPPlus to generate Excel files
                    var package = new OfficeOpenXml.ExcelPackage();
                    var worksheet = package.Workbook.Worksheets.Add("Orders");

                    // Map data to Excel format here
                    var orders = JsonConvert.DeserializeObject<List<OrderDto>>(ordersData);

                    // Set headers for the Excel file
                    worksheet.Cells[1, 1].Value = "Item ID";
                    worksheet.Cells[1, 2].Value = "Quantity";
                    worksheet.Cells[1, 3].Value = "Total Price";
                    worksheet.Cells[1, 4].Value = "Username";

                    // Write data to cells
                    for (int i = 0; i < orders.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = orders[i].ItemId;
                        worksheet.Cells[i + 2, 2].Value = orders[i].Quantity;
                        worksheet.Cells[i + 2, 3].Value = orders[i].TotalPrice;
                        worksheet.Cells[i + 2, 4].Value = orders[i].Username;
                    }

                    // Get current date and time for dynamic file name (formatted as "yyyy-MM-dd_HH-mm-ss")
                    string fileName = $"Orders-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";

                    // Set the content type and header for downloading
                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    _logger.LogError("Error fetching orders: " + response.ReasonPhrase);
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching orders: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }




    }
}
