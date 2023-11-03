using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using PersonalSite.Models;
using System.Diagnostics;
using Microsoft.Extensions.Configuration; //Grants easy access to info in appsettings.json

namespace PersonalSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //Add a field for the Configuration settings in appsettings.json
        private readonly IConfiguration _config;


        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
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

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Resume()
        {
            return View();
        }

        public IActionResult Portfolio()
        {
            return View();
        }

        public IActionResult Links()
        {
            return View();
        }

        [HttpPost] //Denotes this Action will handle POST requests
        public IActionResult Contact(ContactViewModel cvm)
        {
            //When a class has validation attributes, that validation should be checked
            //BEFORE attempting to process any of the data provided.

            if (!ModelState.IsValid)
            {
                //If it's not valid, send the user back to the form. We can also pass the
                //object to the View, so the form will keep the info in it they provided

                return View(cvm);
            }

            //To handle sending the email, we'll need to install a NuGet Package and
            //add a few using statements. We can do this with the following steps: 

            //Create the format for the email we will receive from the contact form
            string message = $"You have received a new email from your site's contact form!<br />" +
                $"Sender: {cvm.Name}<br/>Email: {cvm.Email}<br />Subject: {cvm.Subject}<br />Message: {cvm.Message}";

            //Create a MimeMessage object to assist with storing and transporting the email
            var mm = new MimeMessage();

            //Even though the user is the one attempting to reach us, the actual sender of the email
            //will be the email user we created on our hosting account.

            //We can access the credentials for this email user from appsettings.json as shown below:
            mm.From.Add(new MailboxAddress("Sender", _config.GetValue<string>("Credentials:Email:User")));

            //The recipient of this email is our personal email address:
            mm.To.Add(new MailboxAddress("Personal", _config.GetValue<string>("Credentials:Email:Recipient")));

            //The subject is the one provided by the user:
            mm.Subject = cvm.Subject;

            //The body of the email will be formatted with the string we created above:
            mm.Body = new TextPart("HTML") { Text = message };

            //We can set the priority of the message as "urgent," so it will be flagged in our email client
            mm.Priority = MessagePriority.Urgent;

            //We can also add the user's email address to the list of ReplyTo addresses.
            mm.ReplyTo.Add(new MailboxAddress("User", cvm.Email));

            //The using directive will create the SmtpClient object used to send the email.
            //Once all of the code inside its scope has been executed, it will close any
            //open connections and dispose of the object for us.
            using (var client = new SmtpClient())
            {
                //Connect to the mail server
                //client.Connect(_config.GetValue<string>("Credentials:Email:Client"));
                //if we use the above, without the 8889, some servers reject it
                client.Connect(_config.GetValue<string>("Credentials:Email:Client"), 8889);

                //Log in to the mail server using the credentials for our email user.
                client.Authenticate(

                    //Username:
                    _config.GetValue<string>("Credentials:Email:User"),

                     //Password:
                     _config.GetValue<string>("Credentials:Email:Password")

                    );

                //It's possible the mail server may be down when the user attempts to contact us,
                //or our code may have issues. So, we can "encapsulate" our code to send the message
                //in a try/catch.
                try
                {
                    //Try to send the email
                    client.Send(mm);

                }
                catch (Exception ex)
                {
                    //If there's an issue, we can store an error message in a ViewBag variable
                    //to be displayed in the View

                    ViewBag.ErrorMessage = $"There was an error processing your request. Please try again later.<br />" +
                        $"Error Message: {ex.StackTrace}";

                    //Return the user to the View with their form info
                    return View(cvm);
                }
            }

            //If everything goes well, return a View that displays a confirmation message to the user
            //that their email was sent
            return View("EmailConfirmation", cvm);

        }
    }

}