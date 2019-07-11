using System;
using System.Drawing;

namespace pinball_v3
{
    class Bumper
    {
        /* Un bumper es un círculo, con una posición y
         * un radio. La bola simplemente rebota contra el bumper. 
         * Los bumpers tienen la posición fijada desde el principio. */
        private readonly Vector position;
        private readonly double rad;

        /* Función constructora de un bumper. */
        public Bumper(Vector position, double rad)
        {
            this.position = position.Copy();
            this.rad = rad;
        }

        /* Recibe la pelota antigua y la nueva y realiza la colisión. */
        public bool HandleCollision(Ball oldBall, Ball newBall, double friction)
        {
            /* Comprobamos si la nueva pelota colisionará con el bumper. */
            if (Vector.Distance(this.position, newBall.Position) < newBall.Rad + this.rad)
            {
                this.ApplyCollision(oldBall, friction);
                return true;
            }

            return false;
        }

        /* Aplica a la pelota la colisión con este bumper. */
        private void ApplyCollision(Ball ball, double friction)
        {
            /* Creamos la base en que expresaremos la velocidad de la bola. */
            Vector yaxis = Vector.Subtract(ball.Position, this.position);
            Vector xaxis = yaxis.NewNegativeRotation(Math.PI / 2);
            Matrix2D baseMatrix = new Matrix2D(xaxis, yaxis);

            /* Expresamos la velocidad en la nueva base. */
            Vector newVel = baseMatrix.Inverse().TimesVector(ball.Velocity);

            /* Cambiamos de signo la coordenada y de la nueva velocidad. */
            newVel.X *= friction;
            newVel.Y *= -friction;
            
            /* Reescribimos el vector velocidad en la base canónica
             * y se lo asignamos a la bola. */
            ball.Velocity = baseMatrix.TimesVector(newVel);
        }

        /* Dibuja el bumper en el canvas. */
        public void Draw(Graphics graphics, double height)
        {
            SolidBrush brush = new SolidBrush(Color.Blue);
            float xpos = (float)(this.position.X - this.rad);
            float ypos = (float)(height - this.position.Y - this.rad);
            graphics.FillEllipse(brush, xpos, ypos, 2 * (float)this.rad, 2 * (float)this.rad);
        }
    }
}
