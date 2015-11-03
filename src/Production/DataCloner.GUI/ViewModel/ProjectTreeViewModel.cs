﻿using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel
{
    public class ProjectTreeViewModel : TreeViewLazyItemViewModel
    {
        private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/Images/dcproj.png");

        public ProjectTreeViewModel() : base(null, true)
        {
            Image = _image;
        }
    }
}
