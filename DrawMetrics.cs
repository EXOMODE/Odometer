using Android.Graphics;
using System;
using System.Collections.Generic;

namespace Escorp.Android.Views
{
	internal class DrawMetrics
    {
		private readonly Paint textPaint;
		private readonly IDictionary<char, float> charWidths = new Dictionary<char, float>(256);
		private float charHeight, charBaseline;

		private ScrollingDirection preferredScrollingDirection = ScrollingDirection.Any;

		internal DrawMetrics(Paint textPaint)
		{
			this.textPaint = textPaint;
			Invalidate();
		}

		internal virtual void Invalidate()
		{
			charWidths.Clear();
			Paint.FontMetrics fm = textPaint.GetFontMetrics();
			charHeight = fm.Bottom - fm.Top;
			charBaseline = -fm.Top;
		}

		internal virtual float GetCharWidth(char character)
		{
			if (character == Utils.EmptyChar) return 0;

			if (charWidths.TryGetValue(character, out float value)) return value;

			float width = textPaint.MeasureText(Convert.ToString(character));
			charWidths[character] = width;
			return width;
		}

        internal virtual float CharHeight => charHeight;

        internal virtual float CharBaseline => charBaseline;

        internal virtual ScrollingDirection PreferredScrollingDirection
        {
            get => preferredScrollingDirection;
            set => preferredScrollingDirection = value;
        }
    }
}