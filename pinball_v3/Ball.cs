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

        /* Posición y momento en que estaba la pelota
         * en el último frame. */
        private Vector lastPosition;
        private DateTime lastFrame;

        /* Fijo velocidades límite y gravedad. */
        private readonly double minVelocity = 10; /* px / segundo */
        private readonly double maxVelocity = 400; /* px / segundo */
        private readonly double gravity = 9.8;

        /* Función constructora de la pelota. */
        public Ball(Vector position, Vector velocity, double rad)
        {
            /* Guardo variables. */
            this.position = position.Copy();
            this.lastPosition = position.Copy();
            this.velocity = velocity.Copy();
            this.rad = rad;

            /* Inicializo el último frame. */
            this.lastFrame = DateTime.UtcNow;

            /* Fabrico el polígono de colisión. */
            List<Vector> vertices = new List<Vector>(4);
            vertices.Add(new Vector(position.X - rad, position.Y + rad));
            vertices.Add(new Vector(position.X + rad, position.Y + rad));
            vertices.Add(new Vector(position.X + rad, position.Y - rad));
            vertices.Add(new Vector(position.X - rad, position.Y - rad));
            this.polygon = new Polygon(vertices);
        }

        /* Propiedad de la posición de la pelota. */
        public Vector Position
        {
            get { return this.position; }
            set { this.position = value.Copy(); }
        }

        /* Propiedad de la última posición de la pelota. */
        public Vector LastPosition
        {
            get { return this.lastPosition; }
            set { this.lastPosition = value.Copy(); }
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

        /* Actualiza la posición de la pelota con cada tick del timer. */
        public void Move()
        {
            /* Guardamos la última posición. */
            this.lastPosition = this.position.Copy();

            /* Calculamos el número de segundos que han pasado
             * desde el último frame. */
            double elapsed = DateTime.UtcNow.Subtract(this.lastFrame).TotalMilliseconds; /* milisegundos */

            /* Calculamos el número de pixeles que se tiene que mover la 
             * pelota en este frame. */
            Vector pixelsPerFrame = this.Velocity.NewTimes(elapsed / 1000);
            this.position.Sum(pixelsPerFrame);

            /* Aplicamos los efectos de fricción. */
            if (Math.Abs(this.Velocity.X) > this.minVelocity)
            {
                this.Velocity.X *= Math.Pow(0.4, elapsed / 1000);
            }

            /* Aplicamos los efectos de la gravedad. */
            double gravityIncrease = 400 * this.gravity * (elapsed / 1000) * 0.1;
            this.Velocity.Y -= gravityIncrease * 1;

            /* Actualizamos el instante del último frame, que es ahora. */
            this.lastFrame = DateTime.UtcNow;
        }

        /* Actualiza el polígono de colisión de la pelota. */
        public void UpdateCollisionPolygon()
        {
            List<Vector> vertices = new List<Vector>(4);
            vertices.Add(new Vector(position.X - this.rad, position.Y + this.rad));
            vertices.Add(new Vector(position.X + this.rad, position.Y + this.rad));
            vertices.Add(new Vector(position.X + this.rad, position.Y - this.rad));
            vertices.Add(new Vector(position.X - this.rad, position.Y - this.rad));
            this.polygon = new Polygon(vertices);
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

            /* Reflejo la velocidad sobre el eje mtv. */
            this.Deflect(mtv.axis);

            /* Compruebo que la velocidad no se ha salido de los límites. */
            this.CheckVelocityLimit();
        }

        /* Refleja la velocidad de la pelota sobre el eje y que se le pasa. */
        public void Deflect(Vector yaxis)
        {
            /* Fabrico la matriz de rotación. */
            Vector xaxis = yaxis.NewNegativeRotation(Math.PI / 2);
            Matrix2D basis = new Matrix2D(xaxis, yaxis);

            /* Expreso la velocidad de la pelota en las nuevas coordenadas. */
            Vector projVel = basis.Inverse().TimesVector(this.velocity);

            /* Cambio la coordenada y. */
            projVel.Y *= -1;

            /* Reescribo en la base canónica. */
            this.velocity = basis.TimesVector(projVel);
        }

        /* Comprueba que el eje que marca mtv apunta desde el polígono hacia la pelota, 
         * para sacarla hacia afuera. */
        private void CheckSATAxisDirection(MinimumTranslationVector mtv, Polygon polygon)
        {
            /* Calculo un vector desde el centroide de la pelota hasta el centroide
             * del polígono. */
            Vector ballDirection = Vector.Subtract(polygon.Centroid, this.position);
            ballDirection.Normalize();

            if (Vector.DotProduct(mtv.axis, ballDirection) > 0)
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
            this.lastPosition = this.position.Copy();
            this.position.Sum(displacement);
        }

        /* Comprueba que la velocidad de la pelota no es ni muy baja ni muy alta. */
        // TODO: comprobar que no es muy baja?
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

        /* Decide si la pelota choca contra las paredes del canvas. Si es así,
         * cambia la velocidad de la pelota y la separa de la pared. */
        public bool CheckCanvas(int width, int height)
        {
            /* Compruebo paredes derecha e izquierda. */
            if (this.Position.X - this.Rad <= 0 || this.Position.X + this.Rad >= width)
            {
                /* Modifico la velocidad. */
                this.Velocity.X *= -1;

                /* Separo a la pelota de la pared. */
                if (this.Position.X - this.Rad < 0)
                {
                    this.Position.X -= this.Position.X - this.Rad;
                }
                if (this.Position.X + this.Rad > width)
                {
                    this.Position.X -= this.Position.X + this.Rad - width;
                }

                return true;
            }
            /* Compruebo suelo y techo. */
            else if (this.Position.Y - this.Rad <= 0 || this.Position.Y + this.Rad >= height)
            {
                /* Modifico la velocidad. */
                this.Velocity.Y *= -1;

                /* Separo la pelota de las paredes. */
                if (this.Position.Y - this.Rad < 0)
                {
                    this.Position.Y -= this.Position.Y - this.Rad;
                }
                if (this.Position.Y + this.Rad > height)
                {
                    this.Position.Y -= this.Position.Y + this.Rad - height;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /* Dibuja la pelota al canvas. */
        public void Draw(Graphics graphics, double height)
        {
            /* Transformo posición a posición en el canvas. */
            float xpos = (float)(this.position.X - this.rad);
            float ypos = (float)(height - this.position.Y - this.rad);
            graphics.FillEllipse(new SolidBrush(Color.Red), xpos, ypos, 2 * (float)this.rad, 2 * (float)this.rad);

            // TODO: QUITAR.
            // Dibujo el polígono de colisión.
            this.polygon.Draw(new Pen(Color.Black, 3), graphics, height);
        }
    }
}
