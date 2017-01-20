using System;
using System.Drawing;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Slider
{
    public static class SliderHandler
    {

        public static void SlideWithChrome(string companyName)
        {
            const string url = "http://www.gsxt.gov.cn/index.html";

            //InternetExplorerOptions options = new InternetExplorerOptions();
            ////取消浏览器的保护模式 设置为true
            //options.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
            //这里用chrome浏览器 ie浏览器有问题
            var options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");

            using (var driver = new ChromeDriver(options))
            {
                //设置浏览器大小 设置为最大 元素的X,Y坐标就准了 不然就不准(不知道原因)
                driver.Manage().Window.Maximize();

                var navigation = driver.Navigate();
                navigation.GoToUrl(url);
                var keyWord = driver.FindElement(By.Id("keyword"));
                //keyWord.SendKeys("温州红辣椒电子商务有限公司");
                keyWord.SendKeys(companyName);
                //等待时间
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1000 * 50));
                //等待元素全部加载完成
                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("btn_query")));

                var js = (IJavaScriptExecutor) driver;
                var btnQuery = driver.FindElement(By.Id("btn_query"));
                //经测试，这里要停一下，不然刚得到元素就click可能不会出现滑动块窗口(很坑的地方)
                Thread.Sleep(1000);
                js.ExecuteScript("arguments[0].click();", btnQuery);
                //btnQuery.Click();
                //btnQuery.SendKeys(Keys.Enter);

                //截图加滑动处理
                //因为只有一个弹出窗口，所以直接进到这个里面就好了
                var allWindowsId = driver.WindowHandles;
                if (allWindowsId.Count != 1)
                    throw new Exception("多个弹出窗口。");
                
                foreach (var windowId in allWindowsId)
                {
                    
                    driver.SwitchTo().Window(windowId);
                }

                
                //等待元素全部加载完成
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div.gt_box")));

                //找到图片
                var imageBox = driver.FindElement(By.CssSelector("div.gt_box"));
                //先休息一会，不然截图不对
                Thread.Sleep(1000);
                //截图得到子图片
                var imageFirst = GetSubImage(driver, imageBox);
                imageFirst?.Save("c:/test.png");


                var slide = driver.FindElement(By.CssSelector("div.gt_slider_knob.gt_show"));
                var action = new Actions(driver);
                //移到起始位置
                action.ClickAndHold(slide).MoveByOffset(0, 0).Perform();
                //先休息一会，不然截图不对
                Thread.Sleep(1000);
                //再截图得到子图片
                var imageSecond = GetSubImage(driver, imageBox);
                imageSecond?.Save("c:/test1.png");

                //var random = new Random();
                var left = SliderImageHandler.FindXDiffRectangeOfTwoImage(imageFirst, imageSecond) - 7;
                Console.WriteLine($"减7后等于:{left}");
                if (left <= 0)
                    return;
                var pointsTrace = SliderImageHandler.GeTracePoints(left);

                //移动
                MoveHandler(pointsTrace, action, slide);
                //先休息一会，不然截图不对
                Thread.Sleep(1000);
                var imageThird = GetSubImage(driver, imageBox);
                imageThird?.Save("c:/test2.png");
                

                Console.WriteLine("QAQ");
                Console.WriteLine("OVO");
                Console.WriteLine("休息2秒。");
                Thread.Sleep(1000 * 2);


            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void Test()
        {

            var companyNames = File.ReadAllLines(@"C:\Users\Administrator\Desktop\companyname.txt");
            foreach (var companyName in companyNames)
            {
                SlideWithChrome(companyName);
            }
        }


        private static void MoveHandler(PointTrace[] pointsTrace, Actions action, IWebElement webElement)
        {

            var preY = 0;

            //这里还要研究一下 不要从起始位置开始啦
            var length = pointsTrace.Length;
            for (var i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    action.ClickAndHold(webElement).MoveByOffset(pointsTrace[i].XOffset, pointsTrace[i].YOffset).Perform();
                }
                else if (i < length - 1)
                {
                    action.MoveByOffset(pointsTrace[i].XOffset - pointsTrace[i - 1].XOffset, pointsTrace[i].YOffset - preY).Perform();
                }
                else
                {
                    action.MoveByOffset(pointsTrace[i].XOffset - pointsTrace[i - 1].XOffset, pointsTrace[i].YOffset - preY).Release().Perform();
                }

                //上次的y偏移量，下次要剪掉
                preY = pointsTrace[i].YOffset;


                Thread.Sleep(TimeSpan.FromMilliseconds(pointsTrace[i].SleepTime));


            }

        }

        /// <summary>
        /// BytesToImage
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static Image BytesToImage(byte[] bytes)
        {
            var memoryStream = new MemoryStream(bytes);
            var image = Image.FromStream(memoryStream);
            return image;
        }

        /// <summary>
        /// GetSubImage
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Image GetSubImage(byte[] bytes, int x, int y, int width, int height)
        {
            if (width == 0 || height == 0)
                return null;
            var image = BytesToImage(bytes);
            var bitmap = new Bitmap(image);
            var rectangle = new Rectangle(x, y, width, height);
            var bitmapClone = bitmap.Clone(rectangle, bitmap.PixelFormat);
            return bitmapClone;
        }


        /// <summary>
        /// GetSubImage
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="iWebElement"></param>
        /// <returns></returns>
        private static Image GetSubImage(ChromeDriver driver, IWebElement iWebElement)
        {
            var location = iWebElement.Location;
            var x = location.X;
            var y = location.Y;
            var size = iWebElement.Size;
            var width = size.Width;
            var height = size.Height;
            var screenshot = driver.GetScreenshot();
            //screenshot.SaveAsFile("c:/yuantu.png",ImageFormat.Png);
            var byteArray = screenshot.AsByteArray;
            var image = GetSubImage(byteArray, x, y, width, height);
            return image;
        }
    }
}
