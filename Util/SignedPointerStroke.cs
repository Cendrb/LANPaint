using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Util
{
    public class SignedPointerStroke : SignedStroke
    {
        Timer timer;
        Fader fader;

        Dispatcher dispatcher;
        Action<SignedStroke> removeStroke;

        public int StayTime { get; set; }
        public int FadeTime { get; set; }

        public SignedPointerStroke(SignedStroke signed, int stayTime, int fadeTime)
            : base(signed.StylusPoints, signed.DrawingAttributes)
        {
            Id = signed.Id;
            Owner = signed.Owner;

            StayTime = stayTime;
            FadeTime = fadeTime;

            initialize();
        }

        private void initialize()
        {
            timer = new Timer(StayTime);
            timer.AutoReset = false;
            timer.Elapsed += timer_Elapsed;

            fader = new Fader(this, FadeTime);
        }

        public SignedPointerStroke(StylusPointCollection points, DrawingAttributes attr, int stayTime, int fadeTime)
            : base(points, attr)
        {
            StayTime = stayTime;
            FadeTime = fadeTime;

            initialize();
        }

        public void Fade(Dispatcher d, Action<SignedStroke> rS)
        {
            removeStroke = rS;
            dispatcher = d;
            timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            fader.Fade(dispatcher, removeStroke);
        }

        class Fader
        {
            public int FadeTime { get; set; }
            int updateRate = 15;
            SignedStroke stroke;
            Color activeColor;
            int alphaBaseValue;
            double fadeStepUnits;

            Dispatcher dispatcher;
            Action<SignedStroke> removeStroke;

            public Fader(SignedStroke stroke, int fadeTime)
            {
                FadeTime = fadeTime;
                this.stroke = stroke;
            }

            public void Fade(Dispatcher d, Action<SignedStroke> rS)
            {
                dispatcher = d;
                removeStroke = rS;

                alphaBaseValue = stroke.DrawingAttributes.Color.A;
                fadeStepUnits = ((double)alphaBaseValue) / ((double)FadeTime / (double)updateRate);
                activeColor = stroke.DrawingAttributes.Color;

                Timer updater = new Timer(updateRate);
                updater.AutoReset = true;
                updater.Elapsed += fadeStep;
                updater.Start();
            }

            private void fadeStep(object sender, ElapsedEventArgs e)
            {
                if (activeColor.A - fadeStepUnits < 0)
                {
                    ((Timer)sender).Stop();
                     dispatcher.Invoke(new Action(() => removeStroke(stroke)));
                }
                else
                {
                    activeColor.A -= (byte)fadeStepUnits;
                    dispatcher.Invoke(new Action(() => stroke.DrawingAttributes.Color = activeColor));
                }
            }
        }
    }
}
