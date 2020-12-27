namespace Escorp.Android.Views
{
	internal class CharacterIndices
	{
		private readonly CharacterList outerInstance;

		internal readonly int startIndex;
		internal readonly int endIndex;

		public CharacterIndices(CharacterList outerInstance, int startIndex, int endIndex)
		{
			this.outerInstance = outerInstance;
			this.startIndex = startIndex;
			this.endIndex = endIndex;
		}
	}
}