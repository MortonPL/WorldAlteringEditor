﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.Models;
using TSMapEditor.Models.ArtConfig;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A base class for all object type list panels.
    /// </summary>
    public abstract class ObjectListPanel : XNAPanel
    {
        public ObjectListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            EditorState = editorState;
            Map = map;
            TheaterGraphics = theaterGraphics;
        }

        protected EditorState EditorState { get; }
        protected Map Map { get; }
        protected TheaterGraphics TheaterGraphics { get; }

        public XNASuggestionTextBox SearchBox { get; private set; }
        public TreeView ObjectTreeView { get; private set; }

        private XNADropDown ddOwner;

        public override void Initialize()
        {
            var lblOwner = new XNALabel(WindowManager);
            lblOwner.Name = nameof(lblOwner);
            lblOwner.X = Constants.UIEmptySideSpace;
            lblOwner.Y = Constants.UIEmptyTopSpace;
            lblOwner.Text = "Owner:";
            AddChild(lblOwner);

            ddOwner = new XNADropDown(WindowManager);
            ddOwner.Name = nameof(ddOwner);
            ddOwner.X = lblOwner.Right + Constants.UIHorizontalSpacing;
            ddOwner.Y = lblOwner.Y - 1;
            ddOwner.Width = Width - Constants.UIEmptySideSpace - ddOwner.X;
            AddChild(ddOwner);
            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;

            SearchBox = new XNASuggestionTextBox(WindowManager);
            SearchBox.Name = nameof(SearchBox);
            SearchBox.X = Constants.UIEmptySideSpace;
            SearchBox.Y = ddOwner.Bottom + Constants.UIEmptyTopSpace;
            SearchBox.Width = Width - Constants.UIEmptySideSpace * 2;
            SearchBox.Height = Constants.UITextBoxHeight;
            SearchBox.Suggestion = "Search object... (CTRL + F)";
            AddChild(SearchBox);
            SearchBox.TextChanged += SearchBox_TextChanged;
            SearchBox.EnterPressed += SearchBox_EnterPressed;

            ObjectTreeView = new TreeView(WindowManager);
            ObjectTreeView.Name = nameof(ObjectTreeView);
            ObjectTreeView.Y = SearchBox.Bottom + Constants.UIVerticalSpacing;
            ObjectTreeView.Height = Height - ObjectTreeView.Y;
            ObjectTreeView.Width = Width;
            AddChild(ObjectTreeView);
            ObjectTreeView.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 222), 2, 2);

            EditorState.ObjectOwnerChanged += EditorState_ObjectOwnerChanged;

            base.Initialize();

            RefreshHouseList();

            Parent.ClientRectangleUpdated += Parent_ClientRectangleUpdated;
            RefreshPanelSize();

            InitObjects();
        }

        private void SearchBox_EnterPressed(object sender, EventArgs e)
        {
            ObjectTreeView.FindNode(SearchBox.Text, true);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == SearchBox.Suggestion)
                return;

            ObjectTreeView.FindNode(SearchBox.Text, false);
        }

        protected abstract void InitObjects();

        protected void InitObjectsBase<T>(List<T> objectTypeList, ObjectImage[] textures) where T : TechnoType, IArtConfigContainer
        {
            var sideCategories = new List<TreeViewCategory>();
            for (int i = 0; i < objectTypeList.Count; i++)
            {
                TreeViewCategory category = null;
                var objectType = objectTypeList[i];
                Color remapColor = Color.White;

                if (string.IsNullOrEmpty(objectType.Owner))
                {
                    category = FindOrMakeCategory("Unspecified", sideCategories);
                }
                else
                {
                    string[] owners = objectType.Owner.Split(',');
                    string primaryOwnerName = owners[0];
                    var house = Map.StandardHouses.Find(h => h.ININame == primaryOwnerName);
                    if (house != null)
                    {
                        int actsLike = house.ActsLike;
                        if (actsLike > -1)
                            primaryOwnerName = Map.StandardHouses[actsLike].ININame;
                    }

                    var ownerHouse = Map.Houses.Find(h => h.ININame == primaryOwnerName);
                    if (ownerHouse != null)
                        remapColor = ownerHouse.XNAColor;

                    category = FindOrMakeCategory(primaryOwnerName, sideCategories);
                }

                Texture2D texture = null;
                Texture2D remapTexture = null;
                if (textures != null)
                {
                    if (textures[i] != null)
                    {
                        var frames = textures[i].Frames;
                        if (frames.Length > 0)
                        {
                            // Find the first valid frame and use that as our texture
                            int firstNotNullIndex = Array.FindIndex(frames, f => f != null);
                            if (firstNotNullIndex > -1)
                            {
                                texture = frames[firstNotNullIndex].Texture;
                                if (Constants.HQRemap && objectType.GetArtConfig().Remapable)
                                    remapTexture = textures[i].RemapFrames[firstNotNullIndex].Texture;
                            }
                        }
                    }
                }

                category.Nodes.Add(new TreeViewNode()
                {
                    Text = (objectType.ININame.StartsWith("AI") ? "AI - " : "") + objectType.Name + " (" + objectType.ININame + ")",
                    Texture = texture,
                    RemapTexture = remapTexture,
                    RemapColor = remapColor
                });

                category.Nodes = category.Nodes.OrderBy(n => n.Text).ToList();
            }

            sideCategories.ForEach(c => ObjectTreeView.AddCategory(c));
        }

        private TreeViewCategory FindOrMakeCategory(string categoryName, List<TreeViewCategory> categoryList)
        {
            var category = categoryList.Find(c => c.Text == categoryName);
            if (category != null)
                return category;

            category = new TreeViewCategory() { Text = categoryName };
            categoryList.Add(category);
            return category;
        }

        private void Parent_ClientRectangleUpdated(object sender, EventArgs e)
        {
            RefreshSize();
        }

        private void RefreshPanelSize()
        {
            Width = Parent.Width;
            ddOwner.Width = Width - Constants.UIEmptySideSpace - ddOwner.X;
            SearchBox.Width = Width - Constants.UIEmptySideSpace * 2;
            ObjectTreeView.Width = Width;
        }

        private void DdOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditorState.ObjectOwner = Map.Houses[ddOwner.SelectedIndex];
        }

        private void RefreshHouseList()
        {
            ddOwner.SelectedIndexChanged -= DdOwner_SelectedIndexChanged;

            ddOwner.Items.Clear();
            Map.Houses.ForEach(h => ddOwner.AddItem(h.ININame, h.XNAColor));
            ddOwner.SelectedIndex = Map.Houses.FindIndex(h => h == EditorState.ObjectOwner);

            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;
        }

        private void EditorState_ObjectOwnerChanged(object sender, EventArgs e)
        {
            ddOwner.SelectedIndexChanged -= DdOwner_SelectedIndexChanged;

            ddOwner.SelectedIndex = Map.Houses.FindIndex(h => h == EditorState.ObjectOwner);

            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;
        }
    }
}