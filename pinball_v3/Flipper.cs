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
        private AnimationTimer riseTimer;
        private AnimationTimer fallTimer;

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
            riseTimer = new AnimationTimer(25, "easeout");
            fallTimer = new AnimationTimer(175, "easein");

            /* Establezco ángulos mínimo y máximo. */
            this.minAngle = -Math.PI / 5;
            this.maxAngle = Math.PI / 5;

            /* Creo los vértices distinguiendo derecha e izquierda. */
            List<Vector> vertices = new List<Vector>();
            Vector direction = new Vector(Math.Cos(this.angle), Math.Sin(this.angle));
            direction.SetLength(this.length);
            Vector perp;
            if (this.situation == "left")
            {
                perp = direction.NewPositiveRotation(Math.PI / 2);
                perp.SetLength(this.height);
                Vector begin = Vector.Subtract(this.position, perp.NewWithLength(this.height / 2));
                vertices.Add(begin);
                vertices.Add(Vector.Sum(vertices[0], perp));
                vertices.Add(Vector.Sum(vertices[1], direction));
                vertices.Add(Vector.Subtract(vertices[2], perp));
            }
            else
            {
                perp = direction.NewNegativeRotation(Math.PI / 2);
                perp.SetLength(this.height);
                Vector begin = Vector.Sum(this.position, perp.NewWithLength(this.height / 2));
                vertices.Add(begin);
                vertices.Add(Vector.Subtract(vertices[0], perp));
                vertices.Add(Vector.Sum(vertices[1], direction));
                vertices.Add(Vector.Sum(vertices[2], perp));
            }
            this.polygon = new Polygon(vertices);
        }

        /* Gestiona la colisión de este flipper con una pelota. */
        public bool HandleCollisionSAT(Ball ball)
        {
            /* Si la bola está muy arriba, no comprobamos. */
            if (this.OutOfBounds(ball))
            {
                return false;
            }

            /* Pasamos la gestión de la colisión al polígono. */
            return this.polygon.HandleCollisionSAT(ball);
        }

        /* Recibe una pelota y tiene que comprobar si hay colisión
         * con este flipper por el método de ray casting. */
        public bool HandleCollisionRayCasting(Ball ball)
        {
            /* Si el flipper no se está moviendo, no comprobamos. */
            if (!this.riseTimer.IsRunning() && !this.fallTimer.IsRunning())
            {
                return false;
            }

            /* Marco los límites superior e inferior para la colisión. */
            double top = this.position.Y + Math.Sin(this.maxAngle) * this.length;
            double bottom = this.position.Y + Math.Sin(this.minAngle) * this.length;
            if (ball.Position.Y > top || ball.Position.Y < bottom)
            {
                /* Ni siquiera comprobamos en este caso. */
                return false;
            }

            /* Apunto a un vector en la superficie del flipper. */
            Vector flipperVector;
            if (this.situation == "left")
            {
                flipperVector = Vector.Subtract(this.polygon.Vertices[2], this.polygon.Vertices[1]);
            }
            else
            {
                flipperVector = Vector.Subtract(this.polygon.Vertices[3], this.polygon.Vertices[0]);
            }

            /* Vector desde la pelota antigua hasta la actual. */
            Vector ballVector = Vector.Subtract(ball.Position, ball.LastPosition);

            /* Creo las rectas para los dos vectores. */
            StraightLine ballLine = new StraightLine(ball.Position, ballVector);
            StraightLine flipperLine = new StraightLine(this.position, flipperVector);

            /* Calculo el punto de intersección de ambas líneas. */
            Vector intersection = StraightLine.CalculateIntersection(ballLine, flipperLine);

            /* Marco los límites derecho e izquierdo según la situación del flipper. */
            double left, right;
            if (this.situation == "left")
            {
                left = this.position.X;
                right = this.position.X + this.length;
            }
            else
            {
                left = this.position.X - this.length;
                right = this.position.X;
            }

            /* Comprobamos que el punto de intersección está en el área de colisión. */
            if (intersection.Y < top && intersection.Y > bottom &&
                intersection.X > left && intersection.X < right)
            {
                /* Hay colisión. */
                ball.Position = ball.LastPosition;

                /* Reflejo la velocidad sobre la perpendicular al lado del flipper. */
                ball.Deflect(flipperVector.NewPositiveRotation(Math.PI / 2).NewWithLength(1));

                ball.Velocity.X = ball.Velocity.X * 3.5;
                ball.Velocity.Y = ball.Velocity.Y * 3.5;
                if (ball.Velocity.Y < 0)
                {
                    ball.Velocity.Y = -ball.Velocity.Y;
                }
                if (this.situation == "left" && ball.Velocity.X < 0)
                {
                    ball.Velocity.X = -ball.Velocity.X;
                }
                else if (this.situation == "right" && ball.Velocity.X > 0)
                {
                    ball.Velocity.X = -ball.Velocity.X;
                }

                return true;
            }
            else
            {
                return false;
            }
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
            if (this.riseTimer.IsRunning())
            {
                /* Hay movimiento hacia arriba. */
                if (this.riseTimer.Done())
                {
                    /* Se ha acabado el movimiento hacia arriba. */
                    this.riseTimer.Stop();
                    this.angle = this.TreatAngle(this.maxAngle);
                    this.fallTimer.Start();
                }
                else
                {
                    /* El flipper todavía tiene que subir. */
                    this.angle = this.TreatAngle(this.minAngle + (2 * this.maxAngle / this.riseTimer.Duration) * this.riseTimer.GetElapsedTime());
                }
                this.UpdatePolygon();
            }
            else if (this.fallTimer.IsRunning())
            {
                /* Hay movimiento hacia abajo. */
                if (this.fallTimer.Done())
                {
                    /* Terminar el movimiento hacia abajo. */
                    this.fallTimer.Stop();
                    this.angle = this.TreatAngle(this.minAngle);
                }
                else
                {
                    /* El flipper aún está cayendo. */
                    this.angle = this.TreatAngle(this.maxAngle - (2 * this.maxAngle / this.riseTimer.Duration) * this.riseTimer.GetElapsedTime());
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
            Vector perp;
            if (this.situation == "left")
            {
                perp = direction.NewPositiveRotation(Math.PI / 2);
                perp.SetLength(this.height);
                Vector begin = Vector.Subtract(this.position, perp.NewWithLength(this.height / 2));
                vertices.Add(begin);
                vertices.Add(Vector.Sum(vertices[0], perp));
                vertices.Add(Vector.Sum(vertices[1], direction));
                vertices.Add(Vector.Subtract(vertices[2], perp));
            }
            else
            {
                perp = direction.NewNegativeRotation(Math.PI / 2);
                perp.SetLength(this.height);
                Vector begin = Vector.Sum(this.position, perp.NewWithLength(this.height / 2));
                vertices.Add(begin);
                vertices.Add(Vector.Subtract(vertices[0], perp));
                vertices.Add(Vector.Sum(vertices[1], direction));
                vertices.Add(Vector.Sum(vertices[2], perp));
            }
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
            if (this.riseTimer.IsRunning() || this.fallTimer.IsRunning())
            {
                return;
            }

            this.angle = this.TreatAngle(this.minAngle);
            this.riseTimer.Start();
        }

        /* Dibuja el flipper. */
        public void Draw(Graphics graphics, double canvasHeight)
        {
            /* Dibujo el polígono del flipper con color negro. */
            this.polygon.Draw(new Pen(Color.Black, 3), graphics, canvasHeight);
        }
    }
}
