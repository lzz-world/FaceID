using Face.WPF.Utils;
using Face.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face.WPF.ViewModels
{
    public class VedioViewModel
    {
        public VedioView VedioView { get; set; }

        public VedioViewModel()
        {
            Gl.showImage = (image) =>
            {
                if (VedioView != null && VedioView.videoBox != null)
                    VedioView.videoBox.Image = image;
            };
        }
    }
}
