using dot_battle_signal_bus.scripts.Investigation;
using dot_battle_signal_bus.scripts.Systems;
using Godot;
using Godot.Collections;
using System;



using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dot_battle_signal_bus.scripts.UI
{
    public partial class NotebookWindow : Window
    {
        private enum EditMode
        {
            None,
            Related,
            Source,
            Notes
        }

        [Export] private RichTextLabel CategoriesRTL;
        [Export] private RichTextLabel EntriesRTL;
        [Export] private RichTextLabel EntryDetailsRTL;
        [Export] private TextEdit NotesTextEdit;
        [Export] private Label EditingLabel;


        private EditMode _currentMode;
        private Godot.Collections.Dictionary<string, NotebookEntry> _entries => StoryManager.Instance.NotebookEntries;

        private NotebookEntry _selectedEntry;

        private InvestigationParser Parser  => StoryManager.Instance.Parser;

        public override void _Ready()
        {
            CategoriesRTL.MetaClicked += OnCategoryClicked;
            EntriesRTL.MetaClicked += OnEntryClicked;
            EntryDetailsRTL.MetaClicked += OnEntryDetailsMetaClicked;

            SignalBus.Instance.InvestigationAddedToNoteBook += OnInvestigationAdded;

            BuildCategories();

            ShowTopics();

            CloseRequested += () => Hide();
        }

        private void BuildCategories()
        {
            CategoriesRTL.Text = string.Empty;

            CategoriesRTL.AppendText(
        @"[url=topics]Topics[/url]   [url=locations]Locations[/url]   [url=evidence]Evidence[/url]");
        }

        private void OnCategoryClicked(
    Variant meta)
        {

            string category =
                meta.AsString();

            switch (category)
            {
                case "topics":
                    ShowTopics();
                    EntryDetailsRTL.Clear();
                    break;

                case "locations":
                    ShowLocations();
                    EntryDetailsRTL.Clear();
                    break;

                case "evidence":
                    ShowEvidence();
                    EntryDetailsRTL.Clear();
                    break;
            }
        }

        private void ShowTopics()
        {
            EntriesRTL.Clear();
            string key;
            foreach (var topic in Parser.Topics)
            {
                key = CleanStringFromTags(topic);
                AppendEntry(key);
            }
        }

        private void ShowLocations()
        {
            EntriesRTL.Clear();

            string key;
            foreach (var location in Parser.Locations)
            {
                key = CleanStringFromTags(location);
                AppendEntry("@"+ key);
            }
        }

        private void ShowEvidence()
        {
            EntriesRTL.Clear();
            string key;
            foreach (var evidence in Parser.Evidence)
            {
                key = CleanStringFromTags(evidence);
                AppendEntry(key);
            }
        }

        private void AppendEntry(string key)
        {
            EntriesRTL.AppendText(
                    $"[url={key}]{key}[/url]\n");

        }
        private void OnEntryClicked(
    Variant meta)
        {

            ShowEntry(meta.AsString());
        }

        private void ShowEntry(string id)
        {
            _selectedEntry = _entries[id.ToLower()];

    EntryDetailsRTL.Text =
$"""
[b]{_selectedEntry.Title}[/b]

{_selectedEntry.Description}

[b]Related[/b]   [url=edit_related]Edit Related[/url]
{RenderLinks(_selectedEntry.RelatedText)}

[b]Source[/b]   [url=edit_source]Edit Source[/url]
{RenderLinks(_selectedEntry.SourceText)}

[b]Notes[/b]   [url=edit_notes]Edit Notes[/url]
{RenderLinks(_selectedEntry.Notes)}
""";

            SignalBus.Instance.EmitSignal(
                SignalBus.SignalName
                    .NotebookEntrySelected,
                id);
        }

        private void EditRelated()
        {
            _currentMode = EditMode.Related;

            EditingLabel.Text =
                "Editing Related";

            NotesTextEdit.Text =
                _selectedEntry.RelatedText;
        }

        private void EditSource()
        {
            _currentMode = EditMode.Source;

            EditingLabel.Text =
                "Editing Source";

            NotesTextEdit.Text =
                _selectedEntry.SourceText;
        }


        private void EditNotes()
        {
            _currentMode = EditMode.Notes;

            EditingLabel.Text =
                "Editing Notes";

            NotesTextEdit.Text =
                _selectedEntry.Notes;
        }



        private void OnRelatedClicked(
    Variant meta)
        {

            ShowEntry(meta.AsString());
        }


        private void OnEntryDetailsMetaClicked(
    Variant meta)
        {
            string action = meta.AsString();

            switch (action)
            {
                case "edit_related":
                    EditRelated();
                    return;

                case "edit_source":
                    EditSource();
                    return;

                case "edit_notes":
                    EditNotes();
                    return;
            }

            var result = Parser.Resolve(action);

            if (result == null)
                return;

            ShowEntry(result.Token);
        }


        private void OnInvestigationAdded(
    string token,
    string displayText)
        {
            if (token.StartsWith("#"))
                ShowTopics();

            if (token.StartsWith("@"))
                ShowLocations();

            if (token.StartsWith("!"))
                ShowEvidence();
        }


        private string CleanStringFromTags(string input)
        {
            return Regex.Replace(input, "[#@!]", "");
        }

        private string RenderLinks(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "(none)";

            return Regex.Replace(
                text,
                @"([#@!][a-zA-Z0-9_\-]+)",
                "[url=$1]$1[/url]");
        }

        private void OnSavePressed()
        {
            switch (_currentMode)
            {
                case EditMode.Related:
                    _selectedEntry.RelatedText =
                        NotesTextEdit.Text;
                    break;

                case EditMode.Source:
                    _selectedEntry.SourceText =
                        NotesTextEdit.Text;
                    break;

                case EditMode.Notes:
                    _selectedEntry.Notes =
                        NotesTextEdit.Text;
                    break;
            }
            NotesTextEdit.Clear();

            ShowEntry(_selectedEntry.Id);
        }

    }
}