using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace Escorp.Android.Views
{
    internal class OdometerStyledAttributes
    {
        protected Odometer source;

        public GravityFlags Gravity { get; set; }
        public Color ShadowColor { get; set; }
        public float ShadowDx { get; set; }
        public float ShadowDy { get; set; }
        public float ShadowRadius { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public float TextSize { get; set; }
        public TypefaceStyle TextStyle { get; set; }

        public OdometerStyledAttributes(Odometer source, Resources res)
        {
            this.source = source;
            TextColor = Odometer.DefaultTextColor;
            TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, Odometer.DefaultTextSize, res.DisplayMetrics);
            Gravity = Odometer.DefaultGravity;
        }

        public void ApplyTypedArray(TypedArray arr)
        {
            Gravity = (GravityFlags)arr.GetInt(Resource.Styleable.odometer_android_gravity, (int)Gravity);
            ShadowColor = arr.GetColor(Resource.Styleable.odometer_android_shadowColor, ShadowColor);
            ShadowDx = arr.GetFloat(Resource.Styleable.odometer_android_shadowDx, ShadowDx);
            ShadowDy = arr.GetFloat(Resource.Styleable.odometer_android_shadowDy, ShadowDy);
            ShadowRadius = arr.GetFloat(Resource.Styleable.odometer_android_shadowRadius, ShadowRadius);
            Text = arr.GetString(Resource.Styleable.odometer_android_text);
            TextColor = arr.GetColor(Resource.Styleable.odometer_android_textColor, TextColor);
            TextSize = arr.GetDimension(Resource.Styleable.odometer_android_textSize, TextSize);
            TextStyle = (TypefaceStyle)arr.GetInt(Resource.Styleable.odometer_android_textStyle, (int)TextStyle);
        }
    }
}