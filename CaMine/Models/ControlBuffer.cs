using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaMine.Models
{
    internal class ControlBuffer : TableLayoutPanel
    {
        public void TableLayoutPanelBuffered() { 
            this.DoubleBuffered = true;
        
        }
    }
}
