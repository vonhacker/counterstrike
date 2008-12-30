using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CSL.LevelEditor
{
    public class LevelEditorManager
    {
        private LevelEditorManager() 
        { 

        }

        private String _filePath4MapDescriptor = @"C:\Source\cs\trunk\CounterStrikeLive\CounterStrikeLive\Content\map_dev.xml";
        

        private static LevelEditorManager _levelEditorManager;

        public static LevelEditorManager Instance
        {
            get {
                if (_levelEditorManager == null)
                    _levelEditorManager = new LevelEditorManager();
                return _levelEditorManager;
            }
        }

        private WindowEditor _windowEditor;
        public WindowEditor WindowEditor
        {
            set { _windowEditor = value; }
            get { return _windowEditor; }
        }

        internal void Open()
        {
            _windowEditor.UserControlCanvas.SetControl(_filePath4MapDescriptor);
        }


        internal void SelectCanvas(int canvasIndex)
        {
            _windowEditor.UserControlCanvas.SelectCanvas(canvasIndex);
        }

        internal void RemoveLastStroke()
        {
            _windowEditor.UserControlCanvas.RemoveLastStroke();
        }

        internal void SetSelectMode()
        {
            _windowEditor.UserControlCanvas.SetMode(InkCanvasEditingMode.Select, EditorMode.Select);
        }

        internal void SetPolygon()
        {
            _windowEditor.UserControlCanvas.SetMode(InkCanvasEditingMode.None, EditorMode.Polygon);
        }

        internal void Erase()
        {
            _windowEditor.UserControlCanvas.SetMode(InkCanvasEditingMode.EraseByPoint, EditorMode.Erase);
        }

        internal void Scale(bool shouldAdd)
        {
            _windowEditor.UserControlCanvas.Scale(shouldAdd);
        }

        internal void SelectColor()
        {
            _windowEditor.UserControlCanvas.SelectColor();
        }

        internal void SetPage(bool isPageUp)
        {
            _windowEditor.UserControlCanvas.SetPage(isPageUp);
        }

        internal void Copy()
        {
            _windowEditor.UserControlCanvas.Copy();
        }

        internal void Cut()
        {
            _windowEditor.UserControlCanvas.Cut();
        }

        internal void Paste()
        {
            _windowEditor.UserControlCanvas.Paste();
        }

        internal void KeyB()
        {
            _windowEditor.UserControlCanvas.KeyB();
        }
    }
}
