namespace pinball_v3
{
    class StraightLine
    {
        /* Una línea recta consta de un punto por el que pasa
         * y un vector que es su dirección. */
        private Vector position;
        private Vector direction;

        private double m;
        private double b;

        public StraightLine(Vector position, Vector direction)
        {
            /* Guardo. */
            this.position = position.Copy();
            this.direction = direction.Copy();

            /* Calculo pendiente y ordenada en el origen. */
            Vector secondPoint = Vector.Sum(this.position, this.direction);
            if (this.position.X != secondPoint.X)
            {
                this.m = (this.position.Y - secondPoint.Y) / (this.position.X - secondPoint.X);
            }
            else
            {
                /* La pendiente de una recta vertical es infinita. */
                this.m = double.PositiveInfinity;
            }
            this.b = this.position.Y - this.m * this.position.X;
        }

        /* Devuelve la y correspondiente a la x que se le pasa. */
        public double GetYAt(double x)
        {
            return this.m * x + this.b;
        }

        /* Devuelve la x correspondiente a la y que se le pasa. */
        public double GetXAt(double y)
        {
            if (this.m == 0)
            {
                /* En una recta horizontal, esta x no está definida. */
                return double.NaN;
            }

            return (y - this.b) / this.m;
        }

        /* Función estática que devuelve el punto de intersección de
         * dos rectas que se le pasan. */
        public static Vector CalculateIntersection(StraightLine line1, StraightLine line2)
        {
            /* Calculo el punto x de intersección de ambas. */
            double xIntersection;
            if (line1.m != line2.m)
            {
                xIntersection = (line2.b - line1.b) / (line1.m - line2.m);
            }
            else
            {
                /* Dos rectas paralelas no se cortan, devolvemos nan. */
                return new Vector(double.NaN, double.NaN);
            }

            /* Calculo la coordenada y de la intersección. */
            double yIntersection = line1.m * xIntersection + line1.b;
            return new Vector(xIntersection, yIntersection);
        }
    }
}
