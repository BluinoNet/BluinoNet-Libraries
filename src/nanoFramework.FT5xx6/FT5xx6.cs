﻿using System;
using System.Device.Gpio;
using System.Device.I2c;
//using GHIElectronics.TinyCLR.Devices.Gpio;
//using GHIElectronics.TinyCLR.Devices.I2c;

namespace FocalTech.FT5xx6
{
    public class TouchEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }

        public TouchEventArgs(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public class GestureEventArgs : EventArgs
    {
        public Gesture Gesture { get; }

        public GestureEventArgs(Gesture gesture) => this.Gesture = gesture;
    }

    public enum Gesture
    {
        MoveUp = 0x10,
        MoveLeft = 0x14,
        MoveDown = 0x18,
        MoveRight = 0x1C,
        ZoomIn = 0x48,
        ZoomOut = 0x49,
    }

    public delegate void TouchEventHandler(FT5xx6Controller sender, TouchEventArgs e);
    public delegate void GestureEventHandler(FT5xx6Controller sender, GestureEventArgs e);

    public class FT5xx6Controller : IDisposable
    {
        private readonly byte[] addressBuffer = new byte[1];
        private readonly byte[] read32 = new byte[32];
        private readonly I2cDevice i2c;
        private readonly GpioPin interrupt;

        public event TouchEventHandler TouchDown;
        public event TouchEventHandler TouchUp;
        public event TouchEventHandler TouchMove;
        public event GestureEventHandler GestureReceived;

        public int Width { get; set; }
        public int Height { get; set; }

        public TouchOrientation Orientation { get; set; } = TouchOrientation.Degrees0;

        public int SampleCount { get; set; } = 5;

        public static I2cConnectionSettings GetConnectionSettings()
        {
            var setting = new I2cConnectionSettings(1, 0x38, I2cBusSpeed.FastMode);
            return setting;
        }

        public FT5xx6Controller(I2cDevice i2c, GpioPin interrupt)
        {
            this.i2c = i2c;

            this.interrupt = interrupt;
            this.interrupt.SetPinMode(PinMode.Input);
            this.interrupt.DebounceTimeout = TimeSpan.FromMilliseconds(1);

            //this.interrupt.ValueChangedEdge = GpioPinEdge.FallingEdge;
            this.interrupt.ValueChanged += this.OnInterrupt;
        }


        public void Dispose()
        {
            this.i2c.Dispose();
            this.interrupt.Dispose();
        }

        public enum TouchOrientation
        {
            Degrees0 = 0,
            Degrees90 = 1,
            Degrees180 = 2,
            Degrees270 = 3
        }

        private void OnInterrupt(object sender, PinValueChangedEventArgs e)
        {
            try
            {
                if (e.ChangeType == PinEventTypes.Falling)
                {
                    this.i2c.WriteRead(new SpanByte( this.addressBuffer, 0, 1), new SpanByte( this.read32, 0, this.SampleCount * 6 + 2));

                    if (this.read32[1] != 0 && this.GestureReceived != null)
                        this.GestureReceived(this, new GestureEventArgs((Gesture)this.read32[1]));

                    //We do not read the TD_STATUS register because it returns a touch count _excluding_ touch up events, even though the touch registers contain the proper touch up data.
                    for (var i = 0; i < this.SampleCount; i++)
                    {
                        var idx = i * 6 + 3;
                        var flag = (this.read32[0 + idx] & 0xC0) >> 6;
                        var x = ((this.read32[0 + idx] & 0x0F) << 8) | this.read32[1 + idx];
                        var y = ((this.read32[2 + idx] & 0x0F) << 8) | this.read32[3 + idx];

                        if (this.Orientation != TouchOrientation.Degrees0)
                        {
                            // Need width, height to know do swap x,y
                            if (this.Width == 0 || this.Height == 0)
                                throw new ArgumentException("Width and Height must be not zero");

                            switch (this.Orientation)
                            {
                                case TouchOrientation.Degrees180:
                                    x = this.Width - x;
                                    y = this.Height - y;
                                    break;

                                case TouchOrientation.Degrees270:
                                    var temp = x;
                                    x = this.Width - y;
                                    y = temp;

                                    break;

                                case TouchOrientation.Degrees90:
                                    var tmp = x;
                                    x = y;
                                    y = this.Width - tmp;
                                    break;
                            }
                        }

                        (flag == 0 ? this.TouchDown : flag == 1 ? this.TouchUp : flag == 2 ? this.TouchMove : null)?.Invoke(this, new TouchEventArgs(x, y));

                        if (flag == 3)
                            break;
                    }
                }
            }
            catch
            {
                return; // Don't stop main application
            }
        }
    }
}