using System;
using System.Drawing;
using System.Collections.Generic;

namespace pinball_v3
{
    class Flipper
    {
        /* Posiciones y ángulos. */
        private readonly Vector position;
        private double angle;
        private readonly double minAngle;
        private readonly double maxAngle;

        /* Polígono que constituye al flipper. */
        private Polygon polygon;

        /* Timers de subida y de bajada. */
        private AnimationTimer RiseTimer;
        private AnimationTimer FallTimer;

        /* Otras variables físicas. */
        private readonly double length; /* Longitud del flipper. */
        private readonly double height; /* Grosor total del flipper. */
        private readonly string situation; /* Derecha o izquierda. */

        /* Función constructora de un flipper. */
        public Flipper(string situation, Vector position, double angle, FlipperData data)
        {
            /* Guardo algunas variables. */
            this.length = data.length;
            this.height = data.height;

            this.situation = String.Copy(situation);
            this.position = position.Copy();
            this.angle = angle;

            /* Inicializo los timers. */
            RiseTimer = new AnimationTimer(25, "easeout");
            FallTimer = new AnimationTimer(175, "easein");

            /* Establezco ángulos mínimo y máximo. */
            this.minAngle = -Math.PI / 4;
            this.maxAngle = Math.PI / 4;
            
            /* Creo todos los vértices del flipper como si fuera
             * el de la izquierda. */
            List<Vector> vertices = new List<Vector>();
            Vector direction = new Vector(Math.Cos(this.angle), Math.Sin(this.angle));
            direction.SetLength(this.length);
            Vector perp = direction.NewPositiveRotation(Math.PI / 2);
            perp.SetLength(this.height);
            Vector begin = Vector.Subtract(this.position, perp.NewWithLength(this.height / 2));
            vertices.Add(begin);
            vertices.Add(Vector.Sum(vertices[0], perp));
            vertices.Add(Vector.Sum(vertices[1], direction));
            vertices.Add(Vector.Subtract(vertices[2], perp));
            this.polygon = new Polygon(vertices);
        }

        /* Gestiona la colisión de este flipper con una pelota.
         * (Recibe dos posiciones de la pelota, la actual y la anterior). */
        public bool HandleCollision(Ball oldBall, Ball newBall)
        {
            /* Si la bola está muy arriba, no comprobamos. */
            if (this.OutOfBounds(newBall))
            {
                return false;
            }

            /* Pasamos la gestión de la colisión al polígono. */
            return this.polygon.HandleCollision(oldBall, newBall);
        }

        /* Función que decide si la bola está fuera de rango del flipper. */
        private bool OutOfBounds(Ball ball)
        {
            return this.position.Y + this.length < ball.Position.Y - ball.Rad;
        }

        /* Actualiza el ángulo en que tiene que estar el flipper. Para ello
         * consulta al timer correspondiente el ángulo en que tiene 
         * que posicionarse. */
        public void UpdateFlipperAngle()
        {
            if (this.RiseTimer.IsRunning())
            {
                /* Hay movimiento hacia arriba. */
                if (this.RiseTimer.Done())
                {
                    /* Se ha acabado el movimiento hacia arriba. */
                    this.RiseTimer.Stop();
                    this.angle = this.TreatAngle(this.maxAngle);
                    this.FallTimer.Start();
                }
                else
                {
                    /* El flipper todavía tiene que subir. */
                    this.angle = this.TreatAngle(this.minAngle + (2 * this.maxAngle / this.RiseTimer.Duration) * this.RiseTimer.GetElapsedTime());
                }
                this.UpdatePolygon();
            }
            else if (this.FallTimer.IsRunning())
            {
                /* Hay movimiento hacia abajo. */
                if (this.FallTimer.Done())
                {
                    /* Terminar el movimiento hacia abajo. */
                    this.FallTimer.Stop();
                    this.angle = this.TreatAngle(this.minAngle);
                }
                else
                {
                    /* El flipper aún está cayendo. */
                    this.angle = this.TreatAngle(this.maxAngle - (2 * this.maxAngle / this.RiseTimer.Duration) * this.RiseTimer.GetElapsedTime());
                }

                this.UpdatePolygon();
            }
        }

        /* Después de haber cambiado el ángulo del flipper, actualiza el polígono. */
        private void UpdatePolygon()
        {
            /* Creo la lista con los nuevos vértices. */
            List<Vector> vertices = new List<Vector>();
            Vector direction = new Vector(Math.Cos(this.angle), Math.Sin(this.angle));
            direction.SetLength(this.length);
            Vector perp = direction.NewPositiveRotation(Math.PI / 2);
            perp.SetLength(this.height);
            Vector begin = Vector.Subtract(this.position, perp.NewWithLength(this.height / 2));
            vertices.Add(begin);
            vertices.Add(Vector.Sum(vertices[0], perp));
            vertices.Add(Vector.Sum(vertices[1], direction));
            vertices.Add(Vector.Subtract(vertices[2], perp));

            /* Actualizo el polígono con estos nuevos vértices. */
            this.polygon = new Polygon(vertices);
        }

        /* Recibe un ángulo pensado para el flipper de la izquierda y devuelve un ángulo
         * válido para este flipper en función de su situación. */
        private double TreatAngle(double angle)
        {
            if (this.situation == "left")
            {
                /* El ángulo ya es el adecuado. */
                return angle;
            }
            else
            {
                /* Hay que devolver el ángulo simétrico según el eje y. */
                return Math.PI - angle;
            }

        }

        /* Se ha apretado la tecla de movimiento, así que iniciamos
         * el movimiento hacia arriba si el flipper no se mueve. */
        public void KeyPressed()
        {
            Console.Out.WriteLine("Tecla pulsada: movimiento del flipper");

            /* No registrar la tecla mientras el flipper se está moviendo. */
            if (this.RiseTimer.IsRunning() || this.FallTimer.IsRunning())
            {
                return;
            }

            this.angle = this.TreatAngle(this.minAngle);
            this.RiseTimer.Start();
        }

        /* Dibuja el flipper. */
        public void Draw(Graphics graphics, double canvasHeight)
        {
            /* Dibujo el polígono del flipper con color negro. */
            this.polygon.Draw(new Pen(Color.Black, 3), graphics, canvasHeight);
        }
    }
}
