using System;
using System.Drawing;
using System.Collections.Generic;

namespace pinball_v3
{
    class MinimumTranslationVector
    {
        /* Vector de traslación mínimo devuelto por el algoritmo SAT. */
        public Vector axis;
        public double overlap;

        /* Decide si este vector de traslación mínimo significa
         * una colisión. Tiene que haber un eje definido y un 
         * valor de solapamiento distinto de cero. */
        public bool MeansCollision()
        {
            return this.axis != null && this.overlap != 0;
        }
    }

    class Projection
    {
        /* La proyección de un polígono sobre un eje. */
        public double min;
        public double max;

        public Projection(Vector axis, Polygon polygon)
        {
            /* Ponemos las variables min y max a sus valores iniciales. */
            min = double.MaxValue;
            max = double.MinValue;

            /* Recorro cada vértice del polígono, lo proyecto, y actualizo
             * las variables min y max si es necesario. */
            double value;
            for (int i = 0; i < polygon.Number; i++)
            {
                value = Vector.DotProduct(axis, polygon.Vertices[i]);
                if (value < min)
                {
                    min = value;
                }
                else if (value > max)
                {
                    max = value;
                }
            }

        }

        /* Función estática que recibe dos proyecciones y decide si
         * hay overlap entre ellas o no. Su no hay, devuelve 0, y si
         * hay, devuelve el valor absoluto. */
        public static double Overlap(Projection projection1, Projection projection2)
        {
            if (projection1.max > projection2.min && projection2.max > projection1.min)
            {
                /* Hay solpamiento. */
                return Math.Abs(Math.Min(projection1.max - projection2.min, projection2.max - projection1.min));
            }

            /* No ha habido colisión, devolver 0. */
            return 0;
        }
    }
        

    class Polygon
    {
        /* Un polígono consta de una lista de vértices.
         * El último se une al primero. */
        private List<Vector> vertices;
        private List<Vector> edges;
        private List<Vector> xaxes;
        private List<Vector> yaxes;

        private readonly int number;
        private Vector centroid;

        /* Función constructora de un polígono. */
        public Polygon(List<Vector> vertices)
        {
            this.number = vertices.Count;

            /* Inicializamos todas las listas de este polígono. */
            this.vertices = new List<Vector>(this.number);
            this.edges = new List<Vector>(this.number);
            this.xaxes = new List<Vector>(this.number);
            this.yaxes = new List<Vector>(this.number);

            /* Añado los vértices y calculo sus lados . */
            Vector edge;
            for (int i = 0; i < this.number; i++)
            {
                this.vertices.Add(vertices[i]);
                edge = Vector.Subtract(vertices[(i + 1) % this.number], vertices[i % this.number]);
                this.edges.Add(edge);
                this.xaxes.Add(edge.NewWithLength(1));
                this.yaxes.Add(this.xaxes[i].NewPositiveRotation(Math.PI / 2));
            }

            /* Calculo el centroide. */
            double averageX = 0, averageY = 0;
            for (int i = 0; i < this.number; i++)
            {
                averageX += this.vertices[i].X;
                averageY += this.vertices[i].Y;
            }
            this.centroid = new Vector(averageX / this.number, averageY / this.number);
        }

        /* Propiedad para acceder al número de vértices. */
        public int Number
        {
            get { return this.number; }
        }

        /* Propiedad para acceder y settear los vértices del polígono. */
        public List<Vector> Vertices
        {
            get { return this.vertices; }
            set
            {
                this.vertices.Clear();
                if (this.number != value.Count)
                {
                    return;
                }

                /* Añado los vértices que se han pasado. */
                for (int i = 0; i < this.number; i++)
                {
                    this.vertices.Add(value[i].Copy());
                }

                /* Actualizar el resto de elementos. */
                this.Update();
            }
        }

        /* Propiedad que accede a los ejes x. */
        public List<Vector> Xaxes
        {
            get { return this.xaxes; }
        }

        /* Propiedad que accede a los ejes y. */
        public List<Vector> Yaxes
        {
            get { return this.yaxes; }
        }

        /* Propiedad para acceder al centroide. */
        public Vector Centroid
        {
            get { return this.centroid; }
        }

        /* Actualiza los elementos del polígono una vez que sus vértices han cambiado. */
        private void Update()
        {
            /* Limpio todas las listas. */
            this.edges.Clear();
            this.xaxes.Clear();
            this.yaxes.Clear();

            /* Calculo lados y ejes. */
            Vector edge;
            for (int i = 0; i < this.number; i++)
            {
                edge = Vector.Subtract(vertices[(i + 1) % this.number], vertices[i % this.number]);
                this.edges.Add(edge);
                this.xaxes.Add(edge.NewWithLength(1));
                this.yaxes.Add(this.xaxes[i].NewPositiveRotation(Math.PI / 2));
            }

            /* Calculo el centroide. */
            double averageX = 0, averageY = 0;
            for (int i = 0; i < this.number; i++)
            {
                averageX += this.vertices[i].X;
                averageY += this.vertices[i].Y;
            }
            this.centroid = new Vector(averageX / this.number, averageY / this.number);
        }

        public bool HandleCollision(Ball oldBall, Ball newBall)
        {
            MinimumTranslationVector mtv;

            // TODO: llamar a actualizar al principio del loop, no aqui cada vez.
            newBall.UpdateCollisionPolygon();

            /* Vector de movimiento. */
            Vector displacement = Vector.Subtract(newBall.Position, oldBall.Position);

            /* Aplico el teorema de separación de ejes y guardo el resultado en mtv. */
            mtv = this.SATCollidesWith(newBall, displacement);
            if (mtv.MeansCollision())
            {
                /* El resultado de SAT es una colisión, así que tenemos que rebotar la 
                 * pelota de este polígono. */
                newBall.Bounce(mtv, this);
                return true;
            }

            return false;
        }

        private MinimumTranslationVector SATCollidesWith(Ball ball, Vector displacement)
        {
            /* Declaro el resultado del algoritmo. */
            MinimumTranslationVector result = new MinimumTranslationVector();

            List<Vector> axes = new List<Vector>();
            Vector axisWithMinOverlap = null;
            double minOverlap = double.MaxValue;

            /* Obtengo los ejes de la pelota y añado a la lista de ejes. */
            List<Vector> ballAxes = ball.GetSATAxes();
            for (int i = 0; i < ballAxes.Count; i++)
            {
                axes.Add(ballAxes[i]);
            }

            /* Añado los ejes del polígono a la lista de ejes. */
            for (int j = 0; j < this.yaxes.Count; j++)
            {
                axes.Add(this.yaxes[j]);
            }

            /* Recorro cada eje. */
            int total = ballAxes.Count + this.yaxes.Count;
            Projection polygonProjection;
            Projection ballProjection;
            double overlap;
            for (int k = 0; k < total; k++)
            {
                /* Obtengo las proyecciones de cada figura. */
                polygonProjection = new Projection(axes[k], this);
                ballProjection = new Projection(axes[k], ball.Polygon);

                /* Compruebo si se solapan. */
                overlap = Projection.Overlap(ballProjection, polygonProjection);
                if (overlap == 0)
                {
                    /* Si no hay solapamiento entre estas dos proyecciones, 
                     * entonces no hay colisión. */
                    result.axis = null;
                    result.overlap = 0;
                    return result;
                }
                else if (overlap < minOverlap)
                {
                    /* Me quedo con este. */
                    minOverlap = overlap;
                    axisWithMinOverlap = axes[k];
                }
            }

            /* No ha habido separación en ningún eje, así que devolvemos el mtv. */
            result.axis = axisWithMinOverlap;
            result.overlap = minOverlap;
            return result;
        }
        





        public bool CollisionOld(Ball newBall, Ball oldBall, Polygon oldPolygon, bool isFlipper, ref FlipperCollisionResult result)
        {
            /* Recorro cada vértice de este polígono (que es el nuevo). */
            Vector relPosition;
            double px = 0, py = 0;
            for (int i = 0; i < this.number; i++)
            {
                /* Vector que va desde el vértice actual hasta el centro de la bola. */
                relPosition = Vector.Subtract(newBall.Position, this.vertices[i]);

                /* Proyecciones del vector posición sobre los ejes del lado actual. */
                Vector.FillProjections(relPosition, this.xaxes[i], this.yaxes[i], ref px, ref py);

                /* Decidir en función de los valores de las proyecciones. */
                if (px < 0 && relPosition.CalculateLength() < newBall.Rad)
                {
                    /* La bola colisiona con el vértice. Aplicamos la colisión al polígono antiguo
                     * y a la bola antigua. */
                    oldPolygon.ApplyCollisionWithVertex(oldBall, oldPolygon.vertices[i]);

                    /* Anotamos la distancia al centro. */
                    result.Dist = Vector.Distance(oldPolygon.vertices[i], oldBall.Position);

                    return true;
                }
                else if (px > 0 && px < this.edges[i].CalculateLength())
                {
                    if (py > newBall.Rad)
                    {
                        /* Podemos descartar del todo una colisión con este polígono
                         * porque es convexo. */
                        return false;
                    }
                    else if (py < newBall.Rad && py > -newBall.Rad)
                    {
                        /* Hay colisión con este lado. Aplicamos al polígono antiguo y
                         * a la bola antigua. */
                        oldPolygon.ApplyCollisionWithAxes(oldBall, oldPolygon.xaxes[i], oldPolygon.yaxes[i]);

                        /* Rellenamos valores de up and down si esto es un flipper. */
                        if (isFlipper)
                        {
                            result.Up = i == 0 ? true : false;
                            result.Down = i == 2 ? true : false;

                            /* Calculamos el punto de impacto. Está sobre este segmento, a una 
                             * distancia px del primer vértice. */
                            Vector impactPosition = Vector.Sum(this.vertices[i], this.edges[i].NewWithLength(px));
                            result.Dist = Vector.Distance(result.Center, impactPosition);
                        }
                        return true;
                    }
                }

                /* En este punto no hay colisión con este vértice o lado. */
            }

            /* No ha habido colisión con ningún vértice ni lado. */
            return false;
        }

        private void ApplyCollisionWithVertex(Ball ball, Vector vertex)
        {
            /* Tenemos que calcular primero los ejes de coordenadas para la colisión. */
            Vector yaxis = Vector.Subtract(ball.Position, vertex);
            yaxis.Normalize();
            Vector xaxis = yaxis.NewNegativeRotation(Math.PI / 2);

            /* Llamamos a colisión con los nuevos ejes. */
            this.ApplyCollisionWithAxes(ball, xaxis, yaxis);
        }

        private void ApplyCollisionWithAxes(Ball ball, Vector xaxis, Vector yaxis)
        {
            /* Calculo las coordenadas del vector velocidad en la 
             * base nueva. */
            Matrix2D basisMatrix = new Matrix2D(xaxis, yaxis);
            Vector newVel = basisMatrix.Inverse().TimesVector(ball.Velocity);

            /* Cambio la dirección de la velocidad en el eje y. */
            newVel.Y = newVel.Y * -1;

            /* Vuelvo a escribir este vector en la base canónica. */
            ball.Velocity = basisMatrix.TimesVector(newVel);
        }

        /* Rota este polígono alrededor de un centro, el ángulo
         * indicado. */
        public void Rotate(double angle, Vector center)
        {
            /* Rotamos cada uno de los vértices del polígono. */
            Vector relPosition;
            for (int i = 0; i < this.number; i++)
            {
                relPosition = Vector.Subtract(this.vertices[i], center);
                relPosition.PositiveRotation(angle);
                this.vertices[i] = Vector.Sum(center, relPosition);
            }

            /* Actualizo los lados y las bases del polígono. */
            this.edges.Clear();
            this.xaxes.Clear();
            this.yaxes.Clear();
            Vector edge;
            for (int i = 0; i < this.number; i++)
            {
                this.vertices.Add(vertices[i]);
                edge = Vector.Subtract(vertices[(i + 1) % this.number], vertices[i % this.number]);
                this.edges.Add(edge);
                this.xaxes.Add(edge.NewWithLength(1));
                this.yaxes.Add(this.xaxes[i].NewPositiveRotation(Math.PI / 2));
            }
        }

        /* Dibuja el polígono por pantalla. */
        public void Draw(Pen pen, Graphics graphics, double canvasHeight)
        {
            /* Fabrico primero los puntos. */
            Point[] points = new Point[this.number];
            for (int i = 0; i < this.number; i++)
            {
                points[i] = new Point((int)this.vertices[i].X, (int)(canvasHeight - this.vertices[i].Y));
            }

            graphics.DrawPolygon(pen, points);
        }











        /* Devuelve el número de vertices del polígono. */
        public int GetNumber()
        {
            return this.number;
        }

        /* Recibe unos vértices y los toma como propios. */
        public void SetVertices(Vector[] vertices)
        {
            /* Volver si el número de vértices no es adecuado. */
            if (vertices.Length != this.number)
            {
                return;
            }

            for (int i = 0; i < this.number; i++)
            {
                this.vertices[i] = vertices[i].Copy();
            }
        }

        /* Comprobar si un punto de test está dentro de este polígono. */
        public bool HasInside(Vector test)
        {
            /* Variable que contará cuántos segmentos cortan el rayo a la 
             * derecha desde el punto de test. */
            int count = 0;

            /* Iterar con cada segmento del polígono. */
            Vector vector0;
            Vector vector1;
            for (int i = 0; i < this.number; i++)
            {
                vector0 = this.vertices[i % this.number];
                vector1 = this.vertices[(i + 1) % this.number];

                /* Comprobar que el punto está dentro del scope del segmento actual. */
                if ((vector0.Y > test.Y) != (vector1.Y > test.Y))
                {
                    /* En caso de que sí, comprobar que el punto está a la izquierda del segmento. */
                    /* TODO: hacer una función para hallar este punto del eje x. */
                    if (test.X < vector0.X + (test.Y - vector0.Y) * (vector1.X - vector0.X) / (vector1.Y - vector0.Y))
                    {
                        count++;
                    }
                }
            }

            /* Si el número de segmentos a la derecha del punto de test es par,
             * el punto está fuera del polígono. Si no, dentro */
            return count % 2 == 0 ? false : true;
        }
    }
}
