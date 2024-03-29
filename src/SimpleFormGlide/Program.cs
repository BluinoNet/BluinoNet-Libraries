using GHI.Glide;
using GHI.Glide.Display;
using nanoFramework.M5Core2;
using nanoFramework.M5Stack;
using System;
using System.Diagnostics;
using System.Threading;
using Console = nanoFramework.M5Stack.Console;

namespace SimpleFormGlide
{
    public class Program
    {

        public static void Main()
        {

            Debug.WriteLine("Hello from nanoFramework!");
            TestGlide();
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void TestGlide()
        {
            //use only for latest library version
            #region ESP32
            //int backLightPin = 32;
            //int chipSelect = 14;
            //int dataCommand = 27;
            //int reset = 33;
            //// Add the nanoFramework.Hardware.Esp32 to the solution
            //Configuration.SetPinFunction(19, DeviceFunction.SPI1_MISO);
            //Configuration.SetPinFunction(23, DeviceFunction.SPI1_MOSI);
            //Configuration.SetPinFunction(18, DeviceFunction.SPI1_CLOCK);
            // Adjust as well the size of your screen and the position of the screen on the driver
            //DisplayControl.Initialize(new SpiConfiguration(1, chipSelect, dataCommand, reset, backLightPin), new ScreenConfiguration(0, 0, 320, 240));

            // Depending on you ESP32, you may also have to use either PWM either GPIO to set the backlight pin mode on
            // GpioController.OpenPin(backLightPin, PinMode.Output);
            // GpioController.Write(backLightPin, PinValue.High);
            // Assign GPIO / Key functions to GPIOButtonInputProvider
            // Esp32
            //inputProvider.AddButton(12, Button.VK_LEFT, true);
            //inputProvider.AddButton(13, Button.VK_RIGHT, true);
            //inputProvider.AddButton(34, Button.VK_UP, true);
            //inputProvider.AddButton(35, Button.VK_SELECT, true);
            //inputProvider.AddButton(36, Button.VK_DOWN, true);
            #endregion
            //verify the interupt pin for lcd touch controller
            //var touch = new TouchController(nanoFramework.Hardware.Esp32.Gpio.IO13);
            M5Core2.InitializeScreen(2 * 1024 * 1024);


            GHI.Glide.Glide.SetupGlide(320, 240, 96, 0);
            string GlideXML = @"<Glide Version=""1.0.7""><Window Name=""instance115"" Width=""320"" Height=""240"" BackColor=""dce3e7""><Button Name=""btn"" X=""40"" Y=""60"" Width=""120"" Height=""40"" Alpha=""255"" Text=""Click Me"" Font=""4"" FontColor=""000000"" DisabledFontColor=""808080"" TintColor=""000000"" TintAmount=""0""/><TextBlock Name=""TxtTest"" X=""42"" Y=""120"" Width=""250"" Height=""32"" Alpha=""255"" Text=""TextBlock"" TextAlign=""Left"" TextVerticalAlign=""Top"" Font=""6"" FontColor=""0"" BackColor=""000000"" ShowBackColor=""False""/><TextBlock Name=""TxtTest2"" X=""42"" Y=""150"" Width=""250"" Height=""32"" Alpha=""255"" Text=""TextBlock"" TextAlign=""Left"" TextVerticalAlign=""Top"" Font=""6"" FontColor=""0"" BackColor=""000000"" ShowBackColor=""False""/></Window></Glide>";

            //Resources.GetString(Resources.StringResources.Window)
            Window window = GlideLoader.LoadWindow(GlideXML);



            GHI.Glide.UI.Button btn = (GHI.Glide.UI.Button)window.GetChildByName("btn");
            GHI.Glide.UI.TextBlock txt = (GHI.Glide.UI.TextBlock)window.GetChildByName("TxtTest");
            GHI.Glide.UI.TextBlock txt2 = (GHI.Glide.UI.TextBlock)window.GetChildByName("TxtTest2");
            btn.TapEvent += (object sender) =>
            {
                btn.Text = "Glide NF";
                Debug.WriteLine("Button tapped.");

                window.Invalidate();
                btn.Invalidate();
                //txt.Invalidate();
            };

            bool IsTouched = false;
            GHI.Glide.Glide.MainWindow = window;
            M5Core2.TouchEvent += (object sender, nanoFramework.M5Core2.TouchEventArgs e) =>
            {
                const string StrLB = "LEFT BUTTON PRESSED  ";
                const string StrMB = "MIDDLE BUTTON PRESSED  ";
                const string StrRB = "RIGHT BUTTON PRESSED  ";
                const string StrXY1 = "TOUCHED at X= ";
                const string StrXY2 = ",Y= ";
                const string StrID = ",Id= ";
                const string StrDoubleTouch = "Double touch. ";
                const string StrMove = "Moving... ";
                const string StrLiftUp = "Lift up. ";

                Debug.WriteLine($"Touch Panel Event Received Category= {e.EventCategory} Subcategory= {e.TouchEventCategory}");
                Console.CursorLeft = 0;
                Console.CursorTop = 0;

                Debug.WriteLine(StrXY1 + e.X + StrXY2 + e.Y + StrID + e.Id);
                Console.WriteLine(StrXY1 + e.X + StrXY2 + e.Y + StrID + e.Id + "  ");

                if ((e.TouchEventCategory & TouchEventCategory.LeftButton) == TouchEventCategory.LeftButton)
                {
                    Debug.WriteLine(StrLB);
                    Console.WriteLine(StrLB);
                }
                else if ((e.TouchEventCategory & TouchEventCategory.MiddleButton) == TouchEventCategory.MiddleButton)
                {
                    Debug.WriteLine(StrMB);
                    Console.WriteLine(StrMB);
                }
                else if ((e.TouchEventCategory & TouchEventCategory.RightButton) == TouchEventCategory.RightButton)
                {
                    Debug.WriteLine(StrRB);
                    Console.WriteLine(StrRB);
                }

                if ((e.TouchEventCategory & TouchEventCategory.Moving) == TouchEventCategory.Moving)
                {
                    Debug.WriteLine(StrMove);
                    Console.Write(StrMove);
                    if (!IsTouched)
                    {
                        GlideTouch.RaiseTouchDownEvent(sender, new GHI.Glide.TouchEventArgs(new GHI.Glide.Geom.Point(e.X, e.Y)));
                    }
                    else
                    {
                        GlideTouch.RaiseTouchMoveEvent(sender, new GHI.Glide.TouchEventArgs(new GHI.Glide.Geom.Point(e.X, e.Y)));
                    }
                    IsTouched = true;
                    
                }

                if ((e.TouchEventCategory & TouchEventCategory.LiftUp) == TouchEventCategory.LiftUp)
                {
                    Debug.WriteLine(StrLiftUp);
                    Console.Write(StrLiftUp);
                    GlideTouch.RaiseTouchUpEvent(e.X, e.Y);
                    IsTouched = false;
                }

                if ((e.TouchEventCategory & TouchEventCategory.DoubleTouch) == TouchEventCategory.DoubleTouch)
                {
                    Debug.WriteLine(StrDoubleTouch);
                    Console.Write(StrDoubleTouch);
                }

                Console.WriteLine("                                    ");
                Console.WriteLine("                                    ");
                Console.WriteLine("                                    ");
            };
            GlideTouch.Initialize();
            var count = 0;
            Timer testTimer = new Timer((o) =>
            {
                Debug.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss") + ": blink");
                txt.Text = $"time: {DateTime.UtcNow.ToString("HH:mm:ss")}";
                txt2.Text = $"Welcome to Glide ({count++})";
                window.Invalidate();
                txt.Invalidate();
                txt2.Invalidate();
            }, null, 1000, 1000);
            //touch.CapacitiveScreenReleased += Lcd_CapacitiveScreenReleased;
            //touch.CapacitiveScreenPressed += Lcd_CapacitiveScreenPressed;
            //touch.CapacitiveScreenMove += Lcd_CapacitiveScreenMove;
            //Thread.Sleep(Timeout.Infinite);
        }
        #region Lcd Capacitive Touch Events

        /*
        /// <summary>
        /// Function called when released event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenReleased(object sender, TouchController.TouchEventArgs e)
        {
            Debug.WriteLine("you release the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchUpEvent(e.X, e.Y);
        }

        /// <summary>
        /// Function called when pressed event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenPressed(object sender, TouchController.TouchEventArgs e)
        {
            Debug.WriteLine("you press the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchDownEvent(e.X, e.Y);
        }

        private static void Lcd_CapacitiveScreenMove(object sender, TouchController.TouchEventArgs e)
        {
            Debug.WriteLine("you move finger on the lcd at X:" + e.X + " ,Y:" + e.Y);
            GlideTouch.RaiseTouchMoveEvent(sender, new GHI.Glide.TouchEventArgs(new GHI.Glide.Geom.Point(e.X, e.Y)));
        }
        */
        #endregion
    }
}
