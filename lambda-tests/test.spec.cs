using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightC_TestProject.lambda_tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]  // Enables parallel execution
    public class TestSuite
    {
        public static IEnumerable<TestCaseData> BrowserConfigurations()
        {
            yield return new TestCaseData("chromium");
            yield return new TestCaseData("firefox");
        }

        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        [SetUp]
        public async Task SetUp(string browserType)
        {
            var playwright = await Playwright.CreateAsync();
            _browser = browserType switch
            {
                "chromium" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true }),
                "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true }),
                _ => throw new ArgumentException("Unsupported browser type")
            };
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
        }

        [Test, TestCaseSource(nameof(BrowserConfigurations))]
        public async Task TestScenario_1(string browserType)
        {
            await _page.GotoAsync("https://www.lambdatest.com/selenium-playground");
            var simpleFormDemo = _page.GetByText("Simple Form Demo");
            await simpleFormDemo.ClickAsync();
            string currentUrl = _page.Url;
            Assert.That(currentUrl, Does.Contain("simple-form-demo"));
            string val = "Welcome to LambdaTest";
            var inputTextBox = _page.Locator("input[id=user-message]");
            await inputTextBox.FillAsync(val);
            await _page.Locator("#showInput").ClickAsync();
            var expectedText = await _page.Locator("(//div[@id='user-message']/p)[1]").TextContentAsync();
            Assert.That(expectedText, Is.EqualTo(val));
            //await Expect(expectedText).ToHaveTextAsync(val);
        }

        [Test, TestCaseSource(nameof(BrowserConfigurations))]
        public async Task TestScenario_2(string browserType)
        {
            await _page.GotoAsync("https://www.lambdatest.com/selenium-playground");
            var simpleFormDemo = _page.GetByText("Drag & Drop Sliders");
            await simpleFormDemo.ClickAsync();
            var slider = _page.Locator("//input[@class='sp__range' and @value='15']");
            var expectedSlider = _page.Locator("//input[@class='sp__range' and @value='95']");
            await slider.DragToAsync(expectedSlider);
            Assert.That(await _page.Locator("#rangeSuccess").TextContentAsync(), Is.EqualTo("95"));
            //await Expect(_page.Locator("#rangeSuccess")).ToHaveTextAsync("95");
        }

        [Test, TestCaseSource(nameof(BrowserConfigurations))]
        public async Task TestScenario_3(string browserType)
        {
            await _page.GotoAsync("https://www.lambdatest.com/selenium-playground");
            var simpleFormDemo = _page.GetByText("Input Form Submit");
            await simpleFormDemo.ClickAsync();
            var submitButton = _page.GetByText("Submit");
            await submitButton.ClickAsync();
            var inputField = _page.Locator("#name");
            bool isInvalid = await inputField.EvaluateAsync<bool>("input => input.validity.valueMissing");
            Assert.That(isInvalid, Is.True, "The required field validation did not trigger!");
            string validationMessage = await inputField.EvaluateAsync<string>("input => input.validationMessage");
            Console.WriteLine("Validation Message: " + validationMessage);
            Assert.That(validationMessage, Is.EqualTo("Please fill out this field."));
            await inputField.FillAsync("abcd");
            await _page.Locator("[placeholder=Email]").FillAsync("email@gmail.com");
            await _page.GetByPlaceholder("Password").FillAsync("password123");
            await _page.GetByPlaceholder("Company").FillAsync("TX");
            await _page.GetByPlaceholder("Website").FillAsync("www.google.com");
            await _page.GetByPlaceholder("City").FillAsync("Sirsa");
            await _page.GetByPlaceholder("State").FillAsync("Haryana");
            await _page.GetByPlaceholder("Zip code").FillAsync("12345");
            await _page.GetByPlaceholder("Address 1").FillAsync("Sirsa1");
            await _page.GetByPlaceholder("Address 2").FillAsync("Sirsa2");
            var countryDropdown = _page.Locator("[name=country]");
            await countryDropdown.SelectOptionAsync(new SelectOptionValue { Value = "US" });
            await submitButton.ClickAsync();
            Assert.That(await _page.Locator(".success-msg.hidden").TextContentAsync(), Is.EqualTo("Thanks for contacting us, we will get back to you shortly."));
            //await Expect(_page.Locator(".success-msg.hidden")).ToHaveTextAsync("Thanks for contacting us, we will get back to you shortly.");
        }

        [TearDown]
        public async Task TearDown()
        {
            await _page.CloseAsync();
            await _context.CloseAsync();
            await _browser.CloseAsync();
        }
    }
}
