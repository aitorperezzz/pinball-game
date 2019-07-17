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
            return this.axis != null;
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
                if (value > max)
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

            /* No hay solapamiento, devolver 0. */
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

        /* Gestiona la colisión de este polígono con una pelota. */
        public bool HandleCollisionSAT(Ball ball)
        {
            /* Actualizamos el polígono de colisión de la pelota. */
            ball.UpdateCollisionPolygon();

            /* Aplico el teorema de separación de ejes y guardo el resultado en mtv. */
            MinimumTranslationVector mtv = SATCollision(this, ball.Polygon);
            if (mtv.MeansCollision())
            {
                /* El resultado de SAT es una colisión, así que tenemos que rebotar la 
                 * pelota de este polígono. */
                Console.WriteLine(DateTime.UtcNow + " voy a aplicar bounce");
                ball.Bounce(mtv, this);
                return true;
            }

            return false;
        }

        /* Función estática que recibe dos polígonos y aplica el teorema SAT. Devuelve un 
         * vector de traslación mínimo con la información de la colisión. Si ese vector es null,
         * no ha habido colisión; si tiene valor, la ha habido. */
        private static MinimumTranslationVector SATCollision(Polygon polygon1, Polygon polygon2)
        {
            /* Declaro el resultado del algoritmo, un vector de traslación mínimo. */
            MinimumTranslationVector result = new MinimumTranslationVector();

            List<Vector> axes = new List<Vector>();
            Vector axisWithMinOverlap = null;
            double minOverlap = double.MaxValue;

            /* Añado los ejes del primer polígono. */
            for (int i = 0; i < polygon1.Yaxes.Count; i++)
            {
                axes.Add(polygon1.Yaxes[i]);
            }

            /* Añado los ejes del segundo polígono. */
            for (int j = 0; j < polygon2.Yaxes.Count; j++)
            {
                axes.Add(polygon2.Yaxes[j]);
            }

            /* Recorro cada eje. */
            Projection polygon1Projection;
            Projection polygon2Projection;
            double overlap;
            for (int k = 0; k < axes.Count; k++)
            {
                /* Obtengo las proyecciones de cada figura. */
                polygon1Projection = new Projection(axes[k], polygon1);
                polygon2Projection = new Projection(axes[k], polygon2);

                /* Compruebo si se solapan. */
                overlap = Projection.Overlap(polygon1Projection, polygon2Projection);
                if (overlap == 0)
                {
                    /* No hay solapamiento entre estas dos proyecciones, por
                     * tanto no hay solapamiento. */
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

            /* No ha habido separación en ningún eje, así que hay colisión
             * y devolvemos el mtv relleno con los mejores valores. */
            result.axis = axisWithMinOverlap.Copy();
            result.overlap = minOverlap;
            return result;
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

            // TODO: quitar. Dibujar los ejes de colision.
            for (int j = 0; j < this.yaxes.Count; j++)
            {
                if (j == 0)
                {
                    Vector draw = this.yaxes[j].NewWithLength(50);
                    draw.Draw(pen, graphics, canvasHeight, this.centroid);
                }
                
            }
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
