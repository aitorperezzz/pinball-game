using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pinball_v3
{
    class Master
    {
        /* Esta clase tiene información general del juego
         * y controla los demás elementos. */

        /* Control del tiempo. */
        private DateTime lastFrame; /* Tiempo en que se ejecutó el último frame. */

        public Master()
        {
            this.lastFrame = DateTime.UtcNow;
        }
    }
}
