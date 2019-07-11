using System;
using System.Drawing;
using System.Collections.Generic;

namespace pinball_v3
{
    class Ball
    {
        /* La pelota tiene posición, velocidad y radio.
         * Su radio es lo único que no puede cambiar. 
         * Se le asocia un polígono de colisión, que es un cuadrado
         * a su alrededor. */
        private Vector position;
        private Vector velocity;
        private Polygon polygon;
        private readonly double rad;

        private readonly double minVelocity = 1;
        private readonly double maxVelocity = 50;

        /* Función constructora de la pelota. */
        public Ball(Vector position, Vector velocity, double rad)
        {
            /* Guardo variables. */
            this.position = position.Copy();
            this.velocity = velocity.Copy();
            this.rad = rad;

            /* Fabrico el polígono de colisión. */
            List<Vector> vertices = new List<Vector>(4);
            vertices.Add(new Vector(position.X - rad / 2, position.Y + rad / 2));
            vertices.Add(new Vector(position.X + rad / 2, position.Y + rad / 2));
            vertices.Add(new Vector(position.X + rad / 2, position.Y - rad / 2));
            vertices.Add(new Vector(position.X - rad / 2, position.Y - rad / 2));
            this.polygon = new Polygon(vertices);
        }

        /* Propiedad de la posición de la pelota. */
        public Vector Position
        {
            get { return this.position; }
            set { this.position = value.Copy(); }
        }

        /* Propiedad de la velocidad de la pelota. */
        public Vector Velocity
        {
            get { return this.velocity; }
            set { this.velocity = value.Copy(); }
        }

        /* Propiedad del radio. No se puede cambiar, solo leer. */
        public double Rad
        {
            get { return this.rad; }
        }

        /* Propiedad del polígono de la pelota. */
        public Polygon Polygon
        {
            get { return this.polygon; }
        }

        /* Devuelve una copia de esta pelota. */
        public Ball Copy()
        {
            return new Ball(this.position, this.velocity, this.rad);
        }

        /* Actualiza el polígono de colisión de la pelota. */
        public void UpdateCollisionPolygon()
        {
            List<Vector> vertices = new List<Vector>(4);
            vertices.Add(new Vector(position.X - rad / 2, position.Y + rad / 2));
            vertices.Add(new Vector(position.X + rad / 2, position.Y + rad / 2));
            vertices.Add(new Vector(position.X + rad / 2, position.Y - rad / 2));
            vertices.Add(new Vector(position.X - rad / 2, position.Y - rad / 2));
            this.polygon.Vertices = vertices;
        }

        /* Devuelve una referencia a los ejes que se usan en el algoritmo SAT. */
        public List<Vector> GetSATAxes()
        {
            return this.polygon.Yaxes;
        }

        /* Recibe un vector de traslación mínimo procedente del SAT y un polígono,
         * y tiene que rebotar del polígono según el valor del vector. */
        public void Bounce(MinimumTranslationVector mtv, Polygon polygon)
        {
            /* Compruebo que el eje del mtv es el que lleva
             * la bola hacia afuera del polígono. */
            this.CheckSATAxisDirection(mtv, polygon);
            
            /* Primero separo la pelota del polígono la cantidad
             * requerida por mtv. */
            this.Separate(mtv);

            /* Creo unas coordenadas con mtv. */
            Vector yaxis = mtv.axis;
            Vector xaxis = yaxis.NewNegativeRotation(Math.PI / 2);
            Matrix2D basis = new Matrix2D(xaxis, yaxis);

            /* Expreso la velocidad de la pelota en las nuevas coordenadas. */
            Vector projVel = basis.Inverse().TimesVector(this.velocity);

            /* Cambio la coordenada y. */
            projVel.Y *= -1;

            /* Reescribo en la base canónica. */
            this.velocity = basis.TimesVector(projVel);

            /* Compruebo que la velocidad no se ha salido de los límites. */
            this.CheckVelocityLimit();
        }

        /* Comprueba que el eje que marca mtv apunta desde el polígono hacia la pelota, 
         * para sacarla hacia afuera. */
        private void CheckSATAxisDirection(MinimumTranslationVector mtv, Polygon polygon)
        {
            /* Calculo un vector desde el centroide del polígono
             * hasta el centro de la pelota. */
            Vector outDirection = Vector.Subtract(this.position, polygon.Centroid);
            outDirection.Normalize();

            if (Vector.DotProduct(mtv.axis, outDirection) > 0)
            {
                /* Ambos vectores van en la misma dirección, así que
                 * hay que cambiar el eje de mtv de sentido. */
                mtv.axis.X *= -1;
                mtv.axis.Y *= -1;
            }
        }

        /* Separa la pelota según la información en mtv. */
        private void Separate(MinimumTranslationVector mtv)
        {
            /* Calculo el vector de la separación. */
            Vector displacement = mtv.axis.NewWithLength(mtv.overlap);

            /* Le sumo este vector a la posición de la pelota. */
            this.position = Vector.Sum(this.position, displacement);
        }

        /* Comprueba que la velocidad de la pelota no es ni muy baja ni muy alta. */
        private void CheckVelocityLimit()
        {
            /* Coordenada x. */
            if (velocity.X > maxVelocity)
            {
                velocity.X = maxVelocity;
            }
            else if (velocity.X < -maxVelocity)
            {
                velocity.X = -maxVelocity;
            }
            
            /* Coordenada y. */
            if (velocity.Y > maxVelocity)
            {
                velocity.Y = maxVelocity;
            }
            else if (velocity.Y < -maxVelocity)
            {
                velocity.Y = -maxVelocity;
            }
        }






        /* Función estática que recibe la pelota anterior y la simulación
         * y decide si tiene que aplicar colisión con las paredes del canvas, y 
         * en ese caso las aplica. */
        static public bool CheckCanvas(int width, int height, Ball oldBall, Ball newBall, double friction)
        {
            if (newBall.Position.X - newBall.Rad <= 0 || newBall.Position.X + newBall.rad >= width)
            {
                /* Colisión con paredes derecha o izquierda. */
                oldBall.Velocity = new Vector(oldBall.Velocity.X * -1, oldBall.Velocity.Y);
                return true;
            }
            else if (newBall.Position.Y - newBall.rad <= 0 || newBall.Position.Y + newBall.rad >= height)
            {
                /* Colisión con paredes superior o inferior. */
                oldBall.Velocity = new Vector(oldBall.Velocity.X, oldBall.Velocity.Y * -1);
                return true;
            }
            else
            {
                return false;
            }
        }

        /* La pelota actualiza su posición en función
         * de la velocidad que tiene y la gravedad. */
        public void Move(double gravity, double interval)
        {
            /* Actualizo la posición y la velocidad por los que dan la simulación. */
            this.position = this.SimulateNextPosition(gravity, interval);
            this.velocity = this.SimulateNextVelocity(gravity, interval);
        }

        /* Devuelve el siguiente estado, simulado, de la pelota. */
        public Ball SimulateNextState(double gravity, double interval)
        {
            Vector newPos = this.SimulateNextPosition(gravity, interval);
            Vector newVel = this.SimulateNextVelocity(gravity, interval);
            return new Ball(newPos, newVel, this.rad);
        }

        /* Simula el movimiento de la pelota, dando el siguiente valor
         * de la posición. */
        private Vector SimulateNextPosition(double gravity, double interval)
        {
            double newx = this.position.X + this.velocity.X * interval;
            double newy = this.position.Y + this.velocity.Y * interval - (1 / 2) * gravity * Math.Pow(interval, 2);
            return new Vector(newx, newy);
        }

        /* Simula el siguiente valor de la velocidad y lo devuelve. */
        private Vector SimulateNextVelocity(double gravity, double interval)
        {
            double vely = this.velocity.Y - gravity * interval;
            return new Vector(this.velocity.X, vely);
        }
        

        /* Dibuja la pelota al canvas. */
        public void Draw(Graphics graphics, double height)
        {
            SolidBrush ballBrush = new SolidBrush(Color.Red);
            float xpos = (float)(this.position.X - this.rad);
            float ypos = (float)(height - this.position.Y - this.rad);
            graphics.FillEllipse(ballBrush, xpos, ypos, 2 * (float)this.rad, 2 * (float)this.rad);
        }
    }
}
