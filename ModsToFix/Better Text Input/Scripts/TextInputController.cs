using UnityEngine;
using System;
using System.Text.RegularExpressions;
using Pug.UnityExtensions;

namespace BetterTextInput
{
    public enum CompositionState
    {
        None,
        Composing,
        Completed
    }
    internal class TextInputController : MonoBehaviour
    {
        private static readonly Regex RichPattern = new Regex(@"<\s*u\s*>(.*?)<\s*/\s*u\s*>", RegexOptions.Compiled);
        public PugText pugText;
        private string compositionString = "";
        private int compositionStartCharIndex = -1;
        public CompositionState state { get; private set; } = CompositionState.None;
        public float maxWidth;
        private Func<int> _getCurrentCharIndex;
        private Action<int> _setCurrentCharIndex;
        public int currentCharIndex
        {
            set
            {
                _setCurrentCharIndex(value);
            }
            get
            {
                return _getCurrentCharIndex();
            }
        }
        private GameObject selection;
        private int selectionStartIndex = -1;
        public bool isSelectionKeyPressed { get; private set; } = false;
        public bool isSelected { get; private set; } = false;

        private void Awake()
        {
            selection = UnityEngine.Object.Instantiate(BetterTextMod.characterMark, transform);
            selection.name = "TextSelectionHighlight";

            selection.SetActive(false);
        }

        public void Init(PugText pugText, float maxWidth, Func<int> getCurrentCharIndex, Action<int> setCurrentCharIndex, int? spriteSortingOrder = 0, SpriteMaskInteraction? spriteMaskInteraction = SpriteMaskInteraction.None)
        {
            this.pugText = pugText;
            this.maxWidth = maxWidth;
            this._getCurrentCharIndex = getCurrentCharIndex;
            this._setCurrentCharIndex = setCurrentCharIndex;

            this.selection.transform.position = pugText.transform.position;

            this.Reset();

            var selectionSprite = this.selection.GetComponent<SpriteRenderer>();

            selectionSprite.sortingOrder = spriteSortingOrder ?? 0;
            selectionSprite.maskInteraction = spriteMaskInteraction ?? SpriteMaskInteraction.None;
        }

        public void Reset()
        {
            DeactivateSelection();
            ResetComposition();
        }

        private void ResetComposition()
        {
            this.state = CompositionState.None;
            this.compositionString = "";
            this.compositionStartCharIndex = -1;
        }

        public void Update()
        {
            if (!this.gameObject.activeSelf) return;

            if (isSelectionKeyPressed)
            {
                if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                {
                    isSelectionKeyPressed = false;
                    if (selectionStartIndex == currentCharIndex || !isSelected) this.DeactivateSelection();
                }

                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                {
                    MoveSelection();
                }
                else if (Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.End))
                {
                    isSelected = true;
                }
            }
        }

        public void AppendString(string s)
        {
            string taggedString = $"<u>{s}</u>";
            string textString = this.pugText.GetText();
            if (this.currentCharIndex > textString.Length)
            {
                this.currentCharIndex = textString.Length;
            }

            if (state == CompositionState.None)
            {
                if (isSelected) DeleteSelectedText();
                textString = this.pugText.GetText();
                this.pugText.SetText(textString.Insert(this.currentCharIndex, s));
                this.compositionString = textString.Insert(this.currentCharIndex, taggedString);
                this.compositionStartCharIndex = this.currentCharIndex;
                this.currentCharIndex = this.compositionStartCharIndex + s.Length;
                UpdateCompositionState(CompositionState.Composing);
            }
            else if (state == CompositionState.Composing)
            {
                this.pugText.SetText(RichPattern.Replace(this.compositionString, match => s));
                this.compositionString = RichPattern.Replace(this.compositionString, match => taggedString);
                this.currentCharIndex = this.compositionStartCharIndex + s.Length;
            }
            else if (state == CompositionState.Completed)
            {
                this.pugText.SetText(RichPattern.Replace(this.compositionString, match => s));
                this.compositionString = "";
                this.currentCharIndex = this.compositionStartCharIndex + s.Length;
                UpdateCompositionState(CompositionState.None);
            }


            if (this.pugText.dimensions.width > this.maxWidth)
            {
                this.pugText.SetText(textString);
            }

            this.pugText.Render(false, true);
        }

        public void UpdateCompositionState(CompositionState state)
        {
            this.state = state;
        }

        public float GetCharPositionOf(int charIndex)
        {
            float position = this.pugText.transform.position.x + this.pugText.dimensions.xMin + 0.03125f;
            position += ((charIndex > 0 && charIndex <= this.pugText.localCharacterEndPositions.Count) ? this.pugText.localCharacterEndPositions[charIndex - 1].x : 0f);
            return position;
        }

        public void RenderSelection()
        {
            var anchorPosition = GetCharPositionOf(this.selectionStartIndex);
            var focusPosition = GetCharPositionOf(this.currentCharIndex);
            selection.transform.position = new Vector3(anchorPosition, selection.transform.position.y, selection.transform.position.z);
            selection.transform.localScale = new Vector3((focusPosition - anchorPosition + 0.03125f) * 16, selection.transform.localScale.y, selection.transform.localScale.z);
        }

        public void MoveSelection()
        {
            isSelected = true;
            this.RenderSelection();
        }

        public void InitiateSelection()
        {
            if (selectionStartIndex == -1)
            {
                isSelectionKeyPressed = true;
                selectionStartIndex = this.currentCharIndex;
                RenderSelection();
                selection.SetActive(true);
            }
        }

        public void DeactivateSelection()
        {
            this.selection.transform.SetLocalScale(0f, selection.transform.localScale.y, selection.transform.localScale.z);
            this.selection.SetActive(false);

            this.isSelectionKeyPressed = false;
            this.selectionStartIndex = -1;
            this.isSelected = false;
        }

        public void DeleteSelectedText()
        {
            if (isSelected && pugText.GetTextLength() > 0)
            {
                int startIndex = Math.Min(selectionStartIndex, currentCharIndex);
                int count = Math.Abs(selectionStartIndex - currentCharIndex);

                currentCharIndex = startIndex;

                pugText.SetText(pugText.GetText().Remove(startIndex, count));

                pugText.Render(false, true);

                ResetComposition();
                DeactivateSelection();
            }
        }

        public void CopySelectedText()
        {
            if (isSelected)
            {
                int startIndex = Math.Min(selectionStartIndex, currentCharIndex);
                int count = Math.Abs(selectionStartIndex - currentCharIndex);
                GUIUtility.systemCopyBuffer = pugText.GetText().Substring(startIndex, count);
            }
        }

        public void SelectAllText()
        {
            ResetComposition();
            selectionStartIndex = 0;
            currentCharIndex = pugText.GetTextLength();
            RenderSelection();
            selection.SetActive(true);
            isSelected = true;
        }

        public void MoveToStart()
        {
            if (!isSelectionKeyPressed) DeactivateSelection();
            currentCharIndex = 0;
            RenderSelection();
        }

        public void MoveToEnd()
        {
            if (!isSelectionKeyPressed) DeactivateSelection();
            currentCharIndex = pugText.GetTextLength();
            RenderSelection();
        }

        public void MoveToWord(int direction)
        {
            int newPosition = currentCharIndex;
            string pattern = @"\b\w+\b";
            MatchCollection matches = Regex.Matches(pugText.GetText(), pattern);

            if (direction < 0)
            {
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    if (match.Index < currentCharIndex)
                    {
                        newPosition = match.Index + 1;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    if (match.Index + match.Length > currentCharIndex)
                    {
                        newPosition = match.Index - 1 + match.Length;
                        break;
                    }
                }
            }

            currentCharIndex = newPosition;
        }
    }
}