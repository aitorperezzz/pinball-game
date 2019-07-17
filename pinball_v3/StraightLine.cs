namespace pinball_v3
{
    class StraightLine
    {
        /* Una línea recta consta de un punto por el que pasa
         * y un vector que es su dirección. */
        private Vector position;
        private Vector direction;

        public StraightLine(Vector position, Vector direction)
        {
            this.position = position.Copy();
            this.direction = direction.Copy();
        }

        /* Función estática que devuelve el punto de intersección de
         * dos rectas que se le pasan. */
        public static Vector CalculateIntersection(StraightLine line1, StraightLine line2)
        {
            /* Calculamos las pendientes de las dos rectas. */
            Vector secondPoint1 = Vector.Sum(line1.position, line1.direction);
            Vector secondPoint2 = Vector.Sum(line2.position, line2.direction);
            double m1;
            if (line1.position.X != secondPoint1.X)
            {
                m1 = (line1.position.Y - secondPoint1.Y) / (line1.position.X - secondPoint1.X);
            }
            else
            {
                /* La pendiente de una recta vertical es infinita. */
                m1 = double.PositiveInfinity;
            }
            double m2;
            if (line2.position.X != secondPoint2.X)
            {
                m2 = (line2.position.Y - secondPoint2.Y) / (line2.position.X - secondPoint2.X);
            }
            else
            {
                /* La pendiente de una recta vertical es infinita. */
                m2 = double.PositiveInfinity;
            }

            /* Calculo las ordenadas en el origen de ambas rectas. */
            double b1 = line1.position.Y - m1 * line1.position.X;
            double b2 = line2.position.Y - m2 * line2.position.X;

            /* Calculo el punto x de intersección de ambas. */
            double xIntersection;
            if (m1 != m2)
            {
                xIntersection = (b2 - b1) / (m1 - m2);
            }
            else
            {
                /* Dos rectas paralelas no se cortan, devolvemos nan. */
                return new Vector(double.NaN, double.NaN);
            }

            /* Calculo la coordenada y de la intersección. */
            double yIntersection = m1 * xIntersection + b1;
            return new Vector(xIntersection, yIntersection);
        }
    }
}
