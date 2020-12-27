using Android.Graphics;
using System;

namespace Escorp.Android.Views
{
	internal class Column
    {
		private CharacterList[] characterLists;
		private readonly DrawMetrics metrics;

		private char currentChar = Utils.EmptyChar;
		private char targetChar = Utils.EmptyChar;

		private char[] currentCharacterList;
		private int startIndex;
		private int endIndex;

		private int bottomCharIndex;
		private float bottomDelta;
		private float charHeight;

		private float sourceWidth, currentWidth, targetWidth, minimumRequiredWidth;

		private float currentBottomDelta;
		private float previousBottomDelta;
		private int directionAdjustment;

		internal Column(CharacterList[] characterLists, DrawMetrics metrics)
		{
			this.characterLists = characterLists;
			this.metrics = metrics;
		}

		internal virtual CharacterList[] CharacterLists
        {
            set => characterLists = value;
        }

        internal virtual char TargetChar
        {
            set
            {
                targetChar = value;
                sourceWidth = currentWidth;
                targetWidth = metrics.GetCharWidth(value);
                minimumRequiredWidth = Math.Max(sourceWidth, targetWidth);

                SetCharacterIndices();

                bool scrollDown = endIndex >= startIndex;
                directionAdjustment = scrollDown ? 1 : -1;
                previousBottomDelta = currentBottomDelta;
                currentBottomDelta = 0f;
            }

            get => targetChar;
        }

        internal virtual char CurrentChar => currentChar;


        internal virtual float CurrentWidth
		{
			get
			{
				CheckForDrawMetricsChanges();
				return currentWidth;
			}
		}

		internal virtual float MinimumRequiredWidth
		{
			get
			{
				CheckForDrawMetricsChanges();
				return minimumRequiredWidth;
			}
		}

		private void SetCharacterIndices()
		{
			currentCharacterList = null;

			for (int i = 0; i < characterLists.Length; i++)
			{
				CharacterIndices indices = characterLists[i].GetCharacterIndices(currentChar, targetChar, metrics.PreferredScrollingDirection);

				if (indices != null)
				{
					currentCharacterList = characterLists[i].List;
					startIndex = indices.startIndex;
					endIndex = indices.endIndex;
				}
			}

			if (currentCharacterList == null)
			{
				if (currentChar == targetChar)
				{
					currentCharacterList = new char[] { currentChar };
					startIndex = endIndex = 0;
				}
				else
				{
					currentCharacterList = new char[] { currentChar, targetChar };
					startIndex = 0;
					endIndex = 1;
				}
			}
		}

		internal virtual void OnAnimationEnd()
		{
			CheckForDrawMetricsChanges();
			minimumRequiredWidth = currentWidth;
		}

		private void CheckForDrawMetricsChanges()
		{
			float currentTargetWidth = metrics.GetCharWidth(targetChar);

			if (currentWidth == targetWidth && targetWidth != currentTargetWidth) minimumRequiredWidth = currentWidth = targetWidth = currentTargetWidth;
		}

		internal virtual float AnimationProgress
		{
			set
			{
				if (value == 1f)
				{
					currentChar = targetChar;
					currentBottomDelta = 0f;
					previousBottomDelta = 0f;
				}

				float charHeight = metrics.CharHeight;
				float totalHeight = charHeight * Math.Abs(endIndex - startIndex);
				float currentBase = value * totalHeight;
				float bottomCharPosition = currentBase / charHeight;
				float bottomCharOffsetPercentage = bottomCharPosition - (int)bottomCharPosition;
				float additionalDelta = previousBottomDelta * (1f - value);

				bottomDelta = bottomCharOffsetPercentage * charHeight * directionAdjustment + additionalDelta;
				bottomCharIndex = startIndex + ((int)bottomCharPosition * directionAdjustment);

				this.charHeight = charHeight;
				currentWidth = sourceWidth + (targetWidth - sourceWidth) * value;
			}
		}

		internal virtual void Draw(Canvas canvas, Paint textPaint)
		{
			if (DrawText(canvas, textPaint, currentCharacterList, bottomCharIndex, bottomDelta))
			{
				if (bottomCharIndex >= 0) currentChar = currentCharacterList[bottomCharIndex];

				currentBottomDelta = bottomDelta;
			}

			DrawText(canvas, textPaint, currentCharacterList, bottomCharIndex + 1, bottomDelta - charHeight);
			DrawText(canvas, textPaint, currentCharacterList, bottomCharIndex - 1, bottomDelta + charHeight);
		}

		private bool DrawText(Canvas canvas, Paint textPaint, char[] characterList, int index, float verticalOffset)
		{
			if (index >= 0 && index < characterList.Length)
			{
				canvas.DrawText(characterList, index, 1, 0f, verticalOffset, textPaint);
				return true;
			}

			return false;
		}
	}
}