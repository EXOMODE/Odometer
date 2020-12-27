using System;
using System.Collections.Generic;

namespace Escorp.Android.Views
{
    public static class LevenshteinUtils
    {
        internal const int ActionSame = 0;
        internal const int ActionInsert = 1;
        internal const int ActionDelete = 2;

        public static int[] ComputeColumnActions(char[] source, char[] target, IList<char> supportedCharacters)
        {
            int sourceIndex = 0;
            int targetIndex = 0;

            IList<int> columnActions = new List<int>();

            while (true)
            {
                bool reachedEndOfSource = sourceIndex == source.Length;
                bool reachedEndOfTarget = targetIndex == target.Length;

                if (reachedEndOfSource && reachedEndOfTarget)
                {
                    break;
                }
                else if (reachedEndOfSource)
                {
                    FillWithActions(columnActions, target.Length - targetIndex, ActionInsert);
                    break;
                }
                else if (reachedEndOfTarget)
                {
                    FillWithActions(columnActions, source.Length - sourceIndex, ActionDelete);
                    break;
                }


                bool containsSourceChar = supportedCharacters.Contains(source[sourceIndex]);
                bool containsTargetChar = supportedCharacters.Contains(target[targetIndex]);

                if (containsSourceChar && containsTargetChar)
                {
                    int sourceEndIndex = FindNextUnsupportedChar(source, sourceIndex + 1, supportedCharacters);
                    int targetEndIndex = FindNextUnsupportedChar(target, targetIndex + 1, supportedCharacters);

                    AppendColumnActionsForSegment(columnActions, source, target, sourceIndex, sourceEndIndex, targetIndex, targetEndIndex);

                    sourceIndex = sourceEndIndex;
                    targetIndex = targetEndIndex;
                }
                else if (containsSourceChar)
                {
                    columnActions.Add(ActionInsert);
                    targetIndex++;
                }
                else if (containsTargetChar)
                {
                    columnActions.Add(ActionDelete);
                    sourceIndex++;
                }
                else
                {
                    columnActions.Add(ActionSame);
                    sourceIndex++;
                    targetIndex++;
                }
            }

            int[] result = new int[columnActions.Count];

            for (int i = 0; i < columnActions.Count; i++) result[i] = columnActions[i];

            return result;
        }

        private static int FindNextUnsupportedChar(char[] chars, int startIndex, IList<char> supportedCharacters)
        {
            for (int i = startIndex; i < chars.Length; i++)
            {
                if (!supportedCharacters.Contains(chars[i]))
                {
                    return i;
                }
            }

            return chars.Length;
        }

        private static void FillWithActions(IList<int> actions, int num, int action)
        {
            for (int i = 0; i < num; i++) actions.Add(action);
        }

        private static void AppendColumnActionsForSegment(IList<int> columnActions, char[] source, char[] target, int sourceStart, int sourceEnd, int targetStart, int targetEnd)
        {
            int sourceLength = sourceEnd - sourceStart;
            int targetLength = targetEnd - targetStart;
            int resultLength = Math.Max(sourceLength, targetLength);

            if (sourceLength == targetLength)
            {
                FillWithActions(columnActions, resultLength, ActionSame);
                return;
            }

            int numRows = sourceLength + 1;
            int numCols = targetLength + 1;

            int[][] matrix = RectangularArrays.RectangularIntArray(numRows, numCols);

            for (int i = 0; i < numRows; i++) matrix[i][0] = i;
            for (int j = 0; j < numCols; j++) matrix[0][j] = j;

            int cost;
            int row;
            int col;

            for (row = 1; row < numRows; row++)
            {
                for (col = 1; col < numCols; col++)
                {
                    cost = source[row - 1 + sourceStart] == target[col - 1 + targetStart] ? 0 : 1;
                    matrix[row][col] = Min(matrix[row - 1][col] + 1, matrix[row][col - 1] + 1, matrix[row - 1][col - 1] + cost);
                }
            }

            IList<int> resultList = new List<int>(resultLength * 2);
            row = numRows - 1;
            col = numCols - 1;

            while (row > 0 || col > 0)
            {
                if (row == 0)
                {
                    resultList.Add(ActionInsert);
                    col--;
                }
                else if (col == 0)
                {
                    resultList.Add(ActionDelete);
                    row--;
                }
                else
                {
                    int insert = matrix[row][col - 1];
                    int delete = matrix[row - 1][col];
                    int replace = matrix[row - 1][col - 1];

                    if (insert < delete && insert < replace)
                    {
                        resultList.Add(ActionInsert);
                        col--;
                    }
                    else if (delete < replace)
                    {
                        resultList.Add(ActionDelete);
                        row--;
                    }
                    else
                    {
                        resultList.Add(ActionSame);
                        row--;
                        col--;
                    }
                }
            }

            int resultSize = resultList.Count;

            for (int i = resultSize - 1; i >= 0; i--) columnActions.Add(resultList[i]);
        }

        public static int Min(int first, int second, int third) => Math.Min(first, Math.Min(second, third));
    }
}