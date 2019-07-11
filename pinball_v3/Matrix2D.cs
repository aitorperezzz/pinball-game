using System;

namespace pinball_v3
{
    class Matrix2D
    {
        private readonly Vector firstCol;
        private readonly Vector secondCol;

        /* Función constructora. */
        public Matrix2D(Vector firstCol, Vector secondCol)
        {
            this.firstCol = firstCol.Copy();
            this.secondCol = secondCol.Copy();
        }

        /* Función estática que fabrica la matriz de rotación del ángulo
         * que se le pasa. */
        public static Matrix2D GetRotationMatrix(double angle)
        {
            return new Matrix2D(
                new Vector(Math.Cos(angle), Math.Sin(angle)),
                new Vector(-Math.Sin(angle), Math.Cos(angle))
            );
        }

        /* Devuelve una nueva matriz, la inversa de ésta. */
        public Matrix2D Inverse()
        {
            return this.Adjoint().NewTimes(1 / this.CalculateDeterminant());
        }

        /* Multiplica esta matriz por un número y la devuelve. */
        private Matrix2D NewTimes(double factor)
        {
            return new Matrix2D(this.firstCol.NewTimes(factor), this.secondCol.NewTimes(factor));
        }

        /* Devuelve el determinante de la matriz. */
        public double CalculateDeterminant()
        {
            return this.firstCol.X * this.secondCol.Y - this.firstCol.Y * this.secondCol.X;
        }

        /* Devuelve una nueva matriz que es la adjunta de ésta. */
        private Matrix2D Adjoint()
        {
            Vector newFirstCol = new Vector(this.secondCol.Y, -this.firstCol.Y);
            Vector newSecondCol = new Vector(-this.secondCol.X, this.firstCol.X);
            return new Matrix2D(newFirstCol, newSecondCol);
        }

        /* Multiplica esta matriz por un vector por la derecha y devuelve
         * el nuevo vector resultante. */
        public Vector TimesVector(Vector vector)
        {
            double newx = this.firstCol.X * vector.X + this.secondCol.X * vector.Y;
            double newy = this.firstCol.Y * vector.X + this.secondCol.Y * vector.Y;
            return new Vector(newx, newy);
        }
    }
}
