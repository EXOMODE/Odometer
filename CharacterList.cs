using System;
using System.Collections.Generic;
using System.Linq;

namespace Escorp.Android.Views
{
	internal class CharacterList
    {
		private readonly int numOriginalCharacters;
		private readonly char[] characterList;
		private readonly IDictionary<char, int> characterIndicesMap;

		internal CharacterList(string characterList)
		{
			if (characterList.Contains(Convert.ToString(Utils.EmptyChar))) throw new ArgumentException("You cannot include Utils.EmptyChar in the character list.");

			char[] charsArray = characterList.ToCharArray();
			int length = charsArray.Length;
			numOriginalCharacters = length;
			characterIndicesMap = new Dictionary<char, int>(length);

			for (int i = 0; i < length; i++) characterIndicesMap[charsArray[i]] = i;

			this.characterList = new char[length * 2 + 1];
			this.characterList[0] = Utils.EmptyChar;

			for (int i = 0; i < length; i++)
			{
				this.characterList[1 + i] = charsArray[i];
				this.characterList[1 + length + i] = charsArray[i];
			}
		}

		internal virtual CharacterIndices GetCharacterIndices(char start, char end, ScrollingDirection direction)
		{
			int startIndex = GetIndexOfChar(start);
			int endIndex = GetIndexOfChar(end);

			if (startIndex < 0 || endIndex < 0)
			{
				return null;
			}

			switch (direction)
			{
				case ScrollingDirection.Down:
					if (end == Utils.EmptyChar)
						endIndex = characterList.Length;
					else if (endIndex < startIndex)
						endIndex += numOriginalCharacters;
					break;

				case ScrollingDirection.Up:
					if (startIndex < endIndex) startIndex += numOriginalCharacters;
					break;

				case ScrollingDirection.Any:
					if (start != Utils.EmptyChar && end != Utils.EmptyChar)
					{
						if (endIndex < startIndex)
						{
							int nonWrapDistance = startIndex - endIndex;
							int wrapDistance = numOriginalCharacters - startIndex + endIndex;

							if (wrapDistance < nonWrapDistance) endIndex += numOriginalCharacters;
						}
						else if (startIndex < endIndex)
						{
							int nonWrapDistance = endIndex - startIndex;
							int wrapDistance = numOriginalCharacters - endIndex + startIndex;

							if (wrapDistance < nonWrapDistance) startIndex += numOriginalCharacters;
						}
					}
					break;
			}

			return new CharacterIndices(this, startIndex, endIndex);
		}

        internal virtual char[] SupportedCharacters => characterIndicesMap.Keys.ToArray();

        internal virtual char[] List => characterList;

        private int GetIndexOfChar(char c)
		{
			if (c == Utils.EmptyChar) return 0;
			if (characterIndicesMap.ContainsKey(c)) return characterIndicesMap[c] + 1;

			return -1;
		}

	}
}