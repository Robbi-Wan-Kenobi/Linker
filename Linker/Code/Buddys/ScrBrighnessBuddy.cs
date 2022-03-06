using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Linker.Code.Buddys
{
    public static class ScrBrighnessBuddy
    {
        private static I2cDevice i2CAccel;
        private static Timer timer;

        private static int brightness = 55;
        private static int newValue;

        private static bool stillMoving;

        public static int Brightness
        {
            get { return brightness; }
            set
            {
                newValue = value;

                if(!stillMoving)
                    MoveToNewValue();
            }
        }

        static ScrBrighnessBuddy()
        {
            var task = InitI2C();
            
            timer = new Timer(200);
            timer.Elapsed += Timer_GoToNewValue;

        }

        private static void Timer_GoToNewValue(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();

            MoveToNewValue();
        }


        private static void MoveToNewValue()
        {
            int difference;
            double devision;
            double localBrightness = brightness;

            stillMoving = true;

            if (brightness != newValue)
            {
                difference = newValue - brightness;
                devision = difference / 2.0;

                localBrightness += devision;
                brightness = Convert.ToInt32(localBrightness);

                if (Math.Abs(difference) <= 1)
                {
                    //brightness = Convert.ToInt32(localBrightness);
                    stillMoving = false;
                }
                else
                {
                    timer.Start();
                }
            }
            SetNewBrightness(Convert.ToByte(brightness));
        }


        

        public static void SetNewBrightness(byte newValue)
        {
            if (i2CAccel == null)
            {
                var task = InitI2C();
                task.Wait();
            }

            if (i2CAccel != null)
            {
                byte[] writeBuff = new byte[] { 0x86, newValue }; //backlight address, brightness 0-255
                i2CAccel.Write(writeBuff);
            }
        }




        public static async Task InitI2C()
        {
            //int i = int.Parse(value.ToString());
            string i2cDeviceSelector = I2cDevice.GetDeviceSelector();
            var settings = new I2cConnectionSettings(0x45);
            IReadOnlyList<DeviceInformation> devices = await DeviceInformation.FindAllAsync(i2cDeviceSelector);

            if (devices.Count > 0)
            {
                var screen = await I2cDevice.FromIdAsync(devices[0].Id, settings);
                i2CAccel = screen;

                byte[] writeBuff = new byte[] { 0x86, (byte)(brightness) }; //backlight address, brightness 0-255

                /* Write the register settings */
                try
                {
                    i2CAccel.Write(writeBuff);
                }
                catch (Exception ex)
                {
                    LogBuddy.Log("Screenbrightness failed to initialze, reason: " + ex.Message);
                }
            }
        }
    }
}
