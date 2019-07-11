using System;
using System.Drawing;

namespace pinball_v3
{
    /* Esta clase consta de una coordenada x y otra y. */
    public class Vector
    {
        public double x, y;

        /* Función constructora. */
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /* Propiedad de la coordenada x del vector. */
        public double X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /* Propiedad de la coordenada y del vector. */
        public double Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /* Actualiza el valor de este vector al que se le pasa. */
        public void Set(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /* Función estática que devuelve el vector suma de dos vectores. */
        public static Vector Sum(Vector first, Vector second)
        {
            return new Vector(first.x + second.x, first.y + second.y);
        }

        /* Función estática que devuelve el vector resta de dos vectores. */
        public static Vector Subtract(Vector first, Vector second)
        {
            return new Vector(first.x - second.x, first.y - second.y);
        }

        /* Función estática que devuelve el producto escalar de dos vectores. */
        public static double DotProduct(Vector first, Vector second)
        {
            return first.x * second.x + first.y * second.y;
        }

        /* Función estática que da la distancia entre dos vectores. */
        public static double Distance(Vector begin, Vector end)
        {
            return Vector.Subtract(end, begin).CalculateLength();
        }

        public static void FillProjections(Vector vector, Vector xaxis, Vector yaxis, ref double px, ref double py)
        {
            /* Base generada por los ejes de coordenadas proporcionados. */
            Matrix2D basisMatrix = new Matrix2D(xaxis, yaxis);

            /* Proyecto. */
            Vector projection = basisMatrix.Inverse().TimesVector(vector);

            /* Relleno. */
            px = projection.X;
            py = projection.Y;
        }

        /* Devuelve la longitud de este vector. */
        public double CalculateLength()
        {
            return Math.Sqrt(Math.Pow(this.x, 2) + Math.Pow(this.y, 2));
        }

        /* Rotar este vector positivamente el ángulo que se especifica
         * y devolver un nuevo vector en esa dirección. */
        public Vector NewPositiveRotation(double angle)
        {
            return Matrix2D.GetRotationMatrix(angle).TimesVector(this);
        }

        /* Rotar este vector negativamente el ángulo que se pide 
         * y devolver un nuevo vector en esa dirección. */
        public Vector NewNegativeRotation(double angle)
        {
            return Matrix2D.GetRotationMatrix(-angle).TimesVector(this);
        }

        /* Rotar positivamente este vector y actualizar
         * su dirección a esta nueva.
         */
        public void PositiveRotation(double angle)
        {
            Vector newVector = this.NewPositiveRotation(angle);
            this.x = newVector.x;
            this.y = newVector.y;
        }

        /* Rotar negativamente este vector y actualizar
         * su dirección a esta nueva.
         */
        public void NegativeRotation(double angle)
        {
            Vector newVector = this.NewNegativeRotation(angle);
            this.x = newVector.x;
            this.y = newVector.y;
        }

        /* Rota este punto alrededor del centro que se indica, el ángulo
         * que se indica. */
        public void RotateAround(Vector center, double angle)
        {
            /* Calculamos el vector relativo. */
            Vector relVector = Vector.Subtract(this, center);

            /* Rotamos el vector relativo. */
            relVector.PositiveRotation(angle);

            /* Nueva posición de este vector. */
            Vector newPosition = Vector.Sum(center, relVector);
            this.X = newPosition.X;
            this.Y = newPosition.Y;
        }

        /* Devolver un vector que está en la misma dirección que este
         * pero tiene la longitud que se especifica. */
        public Vector NewWithLength(double length)
        {
            double factor = length / this.CalculateLength();
            return new Vector(this.x * factor, this.y * factor);
        }

        /* Se pasa una longitud y se modifica este vector para que 
         * acabe teniendo la longitud deseada. */
        public void SetLength(double length)
        {
            double factor = length / this.CalculateLength();
            this.x *= factor;
            this.y *= factor;
        }

        /* Normalizar este vector. */
        public void Normalize()
        {
            this.SetLength(1);
        }

        /* Devolver este vector, pero normalizado. */
        public Vector NewNormalize()
        {
            Vector result = this.Copy();
            result.SetLength(1);
            return result;
        }

        /* Actualiza el valor de la posición de este vector
         * al de la suma con el que se le pasa. */
        public void Sum(Vector vector)
        {
            this.x += vector.x;
            this.y += vector.y;
        }

        /* Devuelve una copia de este vector. */
        public Vector Copy()
        {
            return new Vector(this.x, this.y);
        }

        /* Devuelve un nuevo vector con las componentes multiplicadas por
         * el factor. Es un producto por un escalar. */
        public Vector NewTimes(double factor)
        {
            return new Vector(this.x * factor, this.y * factor);
        }

        /* Multiplica este vector por un escalar. */
        public void Times(double scalar)
        {
            this.x *= scalar;
            this.y *= scalar;
        }

        /* Cambia de sentido el vector. */
        public void Reverse()
        {
            this.x *= -1;
            this.y *= -1;
        }

        /* Devuelve la pendiente del vector. */
        public double GetSlope()
        {
            return this.y / this.x;
        }

        /* Devuelve el ángulo que forma el vector con el eje X. */
        public double GetAngle()
        {
            if (this.x == 0)
            {
                return Math.PI / 2;
            }

            return this.y >= 0 ? Math.Atan(this.y / this.x) : Math.Atan(this.y / this.x) + Math.PI;
        }

        /* Dibuja este vector en el canvas como una línea, con los gráficos y boli que 
         * se le pasan, en la posición que se indica. */
        public void Draw(Pen pen, Graphics graphics, double height, Vector position)
        {
            Vector endVector = Vector.Sum(position, this);
            Point begin = new Point((int)position.x, (int)(height - position.y));
            Point end = new Point((int)endVector.x, (int)(height - endVector.y));
            graphics.DrawLine(pen, begin, end);
        }
    }
}
