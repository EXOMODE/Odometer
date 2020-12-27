using Android.Graphics;
using System.Collections.Generic;

namespace Escorp.Android.Views
{
	internal class ColumnManager
    {
		internal readonly List<Column> columns = new List<Column>();
		private readonly DrawMetrics metrics;

		private CharacterList[] characterLists;
		private List<char> supportedCharacters;

        internal ColumnManager(DrawMetrics metrics) => this.metrics = metrics;

        internal virtual void SetCharacterLists(params string[] characterLists)
		{
			this.characterLists = new CharacterList[characterLists.Length];

			for (int i = 0; i < characterLists.Length; i++) this.characterLists[i] = new CharacterList(characterLists[i]);

			supportedCharacters = new List<char>();

			for (int i = 0; i < characterLists.Length; i++) supportedCharacters.AddRange(this.characterLists[i].SupportedCharacters);
			foreach (Column tickerColumn in columns) tickerColumn.CharacterLists = this.characterLists;
		}

        internal virtual CharacterList[] GetCharacterLists() => characterLists;

        internal virtual char[] Text
		{
			set
			{
				if (characterLists == null) SetCharacterLists(Utils.ProvideNumberList, Utils.ProvideAlphabeticalList);

				for (int i = 0; i < columns.Count;)
				{
					Column tickerColumn = columns[i];

					if (tickerColumn.CurrentWidth > 0)
						i++;
					else
						columns.RemoveAt(i);
				}

				int[] actions = LevenshteinUtils.ComputeColumnActions(CurrentText, value, supportedCharacters);
				int columnIndex = 0;
				int textIndex = 0;

				for (int i = 0; i < actions.Length; i++)
				{
					switch (actions[i])
					{
						case LevenshteinUtils.ActionInsert:
							columns.Insert(columnIndex, new Column(characterLists, metrics));
							goto case LevenshteinUtils.ActionSame;

						case LevenshteinUtils.ActionSame:
							columns[columnIndex].TargetChar = value[textIndex];
							columnIndex++;
							textIndex++;
							break;

						case LevenshteinUtils.ActionDelete:
							columns[columnIndex].TargetChar = Utils.EmptyChar;
							columnIndex++;
							break;

						default:
							throw new System.ArgumentException("Unknown action: " + actions[i]);
					}
				}
			}
		}

		internal virtual void OnAnimationEnd()
		{
			for (int i = 0, size = columns.Count; i < size; i++)
			{
				Column column = columns[i];
				column.OnAnimationEnd();
			}
		}

		internal virtual float AnimationProgress
		{
			set
			{
				for (int i = 0, size = columns.Count; i < size; i++)
				{
					Column column = columns[i];
					column.AnimationProgress = value;
				}
			}
		}

		internal virtual float MinimumRequiredWidth
		{
			get
			{
				float width = 0f;

				for (int i = 0, size = columns.Count; i < size; i++) width += columns[i].MinimumRequiredWidth;

				return width;
			}
		}

		internal virtual float CurrentWidth
		{
			get
			{
				float width = 0f;

				for (int i = 0, size = columns.Count; i < size; i++) width += columns[i].CurrentWidth;

				return width;
			}
		}

		internal virtual char[] CurrentText
		{
			get
			{
				int size = columns.Count;
				char[] currentText = new char[size];

				for (int i = 0; i < size; i++) currentText[i] = columns[i].CurrentChar;

				return currentText;
			}
		}

		internal virtual void Draw(Canvas canvas, Paint textPaint)
		{
			for (int i = 0, size = columns.Count; i < size; i++)
			{
				Column column = columns[i];
				column.Draw(canvas, textPaint);
				canvas.Translate(column.CurrentWidth, 0f);
			}
		}

	}
}