using System;
using System.Drawing;
using System.Collections.Generic;

namespace pinball_v3
{
    public struct FlipperCollisionResult
    {
        /* Variables en la estructura. */
        private Vector center;
        private bool up;
        private bool down;
        private double dist;

        /* Getters y setters. */
        public Vector Center
        {
            get { return center; }
            set { center = value.Copy(); }
        }
        public bool Up
        {
            get { return up; }
            set { up = value; }
        }

        public bool Down
        {
            get { return down; }
            set { down = value; }
        }

        public double Dist
        {
            get { return dist; }
            set { dist = value; }
        }
    }

    class Flipper
    {
        /* Estructura con variables fijadas. */
        private readonly FlipperData flipperData;

        /* Posiciones y ángulos. */
        private readonly Vector position;
        private double angle;
        private readonly double minAngle;
        private readonly double maxAngle;

        /* Polígono que constituye al flipper. */
        private Polygon polygon;

        /* Otras variables físicas. */
        private readonly double length; /* Longitud del flipper. */
        private readonly double height; /* Grosor total del flipper. */
        private readonly string situation; /* Derecha o izquierda. */
        private bool movingUp; /* Si se está moviendo hacia arriba. */
        private bool movingDown; /* Si se está moviendo hacia abajo. */
        private double interval; /* Intervalo de tiempo de refresco. */
        private readonly double angleRot; /* Ángulo que rota en cada intervalo. */
        private readonly double omegaRot; /* Velocidad angular. */

        /* Función constructora de un flipper. */
        public Flipper(string situation, Vector position, double angle, FlipperData data)
        {
            /* Guardo algunas variables. */
            this.flipperData = data;
            this.length = data.length;
            this.height = data.height;
            this.interval = data.interval;

            this.situation = String.Copy(situation);
            this.position = position.Copy();
            this.angle = angle;

            /* Establezco ángulos mínimo y máximo. */
            if (this.situation == "left")
            {
                this.minAngle = -Math.PI / 5;
                this.maxAngle = Math.PI / 5;
            }
            else
            {
                this.minAngle = Math.PI + Math.PI / 5;
                this.maxAngle = Math.PI - Math.PI / 5;
            }
            
            /* El flipper empieza quieto. */
            this.movingUp = false;
            this.movingDown = false;

            /* Inicio variables relacionadas con la rotación. */
            this.angleRot = 0.2;
            this.omegaRot = this.angleRot / this.interval;

            /* Inicializo el polígono que formará el flipper. */

            /* Primero creo unos ejes de coordenadas. */
            Vector xaxis = new Vector(1, 0);
            xaxis.PositiveRotation(this.angle);
            Vector yaxis = xaxis.NewPositiveRotation(Math.PI / 2);

            /* Creo todos los vértices del flipper. */
            List<Vector> vertices = new List<Vector>();
            vertices.Add(Vector.Sum(this.position, yaxis.NewTimes(-this.height / 2)));
            vertices.Add(Vector.Sum(vertices[0], xaxis.NewTimes(this.length)));
            vertices.Add(Vector.Sum(vertices[1], yaxis.NewTimes(this.height)));
            vertices.Add(Vector.Sum(vertices[0], yaxis.NewTimes(this.height)));
            this.polygon = new Polygon(vertices);
        }

        public bool HandleCollision(Ball oldBall, Ball newBall, double friction)
        {
            return this.polygon.HandleCollision(oldBall, newBall);
        }

        /* Recibe las posiciones antigua y nueva de la pelota y gestiona la 
         * colisión. */
        //public bool HandleCollisionOld(Ball oldBall, Ball newBall, double friction)
        //{
        //    /* Si la nueva bola está muy por encima del flipper, directamente
        //     * no comprobaremos porque no va a haber colisión. */
        //    if (this.OutOfBounds(newBall))
        //    {
        //        this.Move();
        //        return false;
        //    }

        //    /* Creamos un nuevo flipper con la nueva posición. */
        //    double newAngle = this.CalculateNextAngle(false);
        //    Flipper newFlipper = new Flipper(this.situation, this.position, newAngle, this.flipperData);

        //    /* Comprobar si la nueva pelota y el nuevo flipper van a colisionar. */
        //    FlipperCollisionResult result = new FlipperCollisionResult();
        //    result.Center = this.position;
        //    if (newFlipper.Collision(newBall, oldBall, this, ref result))
        //    {
        //        if (result.Up && this.movingUp)
        //        {
        //            /* Indicar al flipper que empiece a moverse hacia abajo. */
        //            this.KeyReleased();
        //        }

        //        /* Si la colisión se ha producido arriba o abajo del flipper,
        //         * realizar algo más de lógica si es necesario. */
        //        if (result.Up && this.movingUp || result.Down && this.movingDown)
        //        {
        //            Vector linearVelVector;
        //            if (result.Up)
        //            {
        //                linearVelVector = this.polygon.Xaxes[3].NewWithLength(this.omegaRot * result.Dist);
        //            }
        //            else
        //            {
        //                linearVelVector = this.polygon.Xaxes[1].NewWithLength(this.omegaRot * result.Dist);
        //            }
        //            Vector newVel = Vector.Sum(oldBall.Velocity, linearVelVector);
        //            oldBall.Velocity = newVel.NewTimes(friction);
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        /* Si no van a colisionar, puedo mover el flipper con libertad. */
        //        this.Move();
        //        return false;
        //    }
        //}

        /* Devuelve el ángulo en el que estará el flipper en el siguiente 
         * instante. */
        private double CalculateNextAngle(bool apply)
        {
            if (!this.movingUp && !this.movingDown)
            {
                /* El flipper no se mueve, devolver el ángulo actual. */
                return this.angle;
            }

            if (this.movingUp)
            {
                if (this.situation == "left")
                {
                    if (this.angle < this.maxAngle)
                    {
                        return this.angle + this.angleRot;
                    }

                    if (apply)
                    {
                        this.movingUp = false;
                        this.movingDown = true;
                    }
                    return this.maxAngle;
                }
                else
                {
                    if (this.angle > this.maxAngle)
                    {
                        return this.angle - this.angleRot;
                    }

                    if (apply)
                    {
                        this.movingUp = false;
                        this.movingDown = true;
                    }
                    return this.maxAngle;
                }
            }
            else if (this.movingDown)
            {
                if (this.situation == "left")
                {
                    if (this.angle > this.minAngle)
                    {
                        return this.angle - this.angleRot;
                    }

                    if (apply)
                    {
                        this.movingUp = false;
                        this.movingDown = false;
                    }
                    return this.minAngle;
                }
                else
                {
                    if (this.angle < this.minAngle)
                    {
                        return this.angle + this.angleRot;
                    }

                    if (apply)
                    {
                        this.movingUp = false;
                        this.movingDown = false;
                    }
                    return this.minAngle;
                }
            }
            return this.angle;
        }

        /* Función de colisión del flipper, que llama a la de su polígono. */
        //private bool Collision(Ball newBall, Ball oldBall, Flipper oldFlipper, ref FlipperCollisionResult result)
        //{
        //    return this.polygon.Collision(newBall, oldBall, oldFlipper.polygon, true, ref result);
        //}

        /* Función que decide si la bola está fuera de rango del flipper. */
        private bool OutOfBounds(Ball ball)
        {
            return this.position.Y + this.length < ball.Position.Y - ball.Rad;
        }

        /* Mueve el flipper a la siguiente posición que le corresponde. Se hace
         * siempre, se supone que la comprobación ya está hecha. */
        public void Move()
        {
            double newAngle = this.CalculateNextAngle(true);

            /* Rotamos el polígono alrededor del centro la cantidad necesaria. */
            this.polygon.Rotate(newAngle - this.angle, this.position);

            this.angle = newAngle;
        }

        /* Se ha apretado la tecla de movimiento, así que iniciamos
         * el movimiento hacia arriba. */
        public void KeyPressed()
        {
            Console.Out.WriteLine("tecla pulsada");
            this.movingUp = true;
            this.movingDown = false;
        }

        /* La tecla de movimiento ha sido soltada, así que iniciamos
         * el movimiento hacia abajo. */
        public void KeyReleased()
        {
            Console.Out.WriteLine("tecla levantada");
            this.movingUp = false;
            this.movingDown = true;
        }

        /* Dibuja el flipper. */
        public void Draw(Graphics graphics, double canvasHeight)
        {
            Pen pen = new Pen(Color.Black, 3);

            /* Dibujo el polígono del flipper. */
            this.polygon.Draw(pen, graphics, canvasHeight);
        }
















        // CODIGO ANTIGUO

        /* Función estática que devuelve una nueva pared dados una posición de flipper, 
         * una dirección, y una situación. */
        //private static Wall CalculateWall(Vector position, Vector direction, string situation, double rad)
        //{
        //    Vector[] ends = new Vector[2];
        //    if (situation == "left")
        //    {
        //        ends[0] = position;
        //        ends[1] = Vector.Sum(position, direction);
        //    }
        //    else
        //    {
        //        ends[0] = Vector.Sum(position, direction);
        //        ends[1] = position;
        //    }
        //    return new Wall(ends, rad);
        //}

        /* Función que gestiona todo el comportamiento del flipper con la
         * pelota actual y la nueva. */
        //public bool HandleBall(Ball ball, Ball newBall)
        //{
        //    /* Si la nueva pelota está lejos del flipper por arriba, no hay colisión. */
        //    if (this.OutOfBounds(newBall))
        //    {
        //        this.Move();
        //        return false;
        //    }

        //    /* Actualizar la pared del flipper para comprobaciones. */
        //    this.wall = Flipper.CalculateWall(this.position, this.direction, this.situation, this.rad);

        //    /* Simular la nueva dirección del flipper y crear la nueva pared. */
        //    Vector newDirection = this.SimulateNextDirection();
        //    Wall newWall = Flipper.CalculateWall(this.position, newDirection, this.situation, this.rad);

        //    /* Comprobar la colisión entre el nuevo flipper y la nueva pelota. */
        //    if (newWall.CheckCollision(newBall))
        //    {
        //        /* Hay que colisionar el antiguo flipper con la antigua pelota. */
        //        this.ApplyCollision(ball);
        //        return true;
        //    }
        //    else
        //    {
        //        /* Actualizar el flipper a la nueva dirección e indicar que 
        //         * no ha habido colisión. */
        //        this.Move();
        //        return false;
        //    }
        //}

        

        /* Aplica la colisión de este flipper con la bola. */
        //public void ApplyCollision(Ball ball)
        //{
        //    /* Primero actualizamos la pelota a la velocidad que tendría
        //     * si solamente chocase contra una pared inmóvil. */
        //    this.wall.ApplyCollision(ball);

        //    /* Ahora le damos más velocidad en función de la distancia al centro del flipper.
        //     * Solo lo hacemos si el flipper se está moviendo hacia arriba, no si está quieto o se 
        //     * está moviendo hacia abajo. */
        //    /* TODO: qué hacer cuando se mueve hacia abajo? */
        //    if (this.movingUp)
        //    {
        //        double dist = Vector.Distance(this.position, ball.Position);
        //        double speed = this.omega * dist;
        //        Vector speedVector = this.wall.GetYAxis();
        //        speedVector.SetLength(speed);
        //        ball.Velocity = Vector.Sum(ball.Velocity, speedVector);
        //    }

        //    /* Parar provisionalmente el flipper. */
        //    this.KeyReleased();
        //}

        /* Da la nueva dirección del flipper, obviando la presencia de
         * una pelota. */
        //private Vector SimulateNextDirection()
        //{
        //    if (this.movingUp)
        //    {
        //        if (!this.ReachedUpperLimit())
        //        {
        //            /* Moverlo una posición hacia arriba. */
        //            return this.RotateUp();
        //        }
        //        else
        //        {
        //            /* Situarlo en la dirección máxima. */
        //            return this.maxDirection.Copy();
        //        }
        //    }
        //    else if (this.movingDown)
        //    {
        //        if (!this.ReachedLowerLimit())
        //        {
        //            /* Tengo que moverlo una posición más hacia abajo. */
        //            return this.RotateDown();
        //        }
        //        else
        //        {
        //            /* Situarlo en la dirección mínima. */
        //            return this.minDirection.Copy();
        //        }
        //    }

        //    /* Si no se está moviendo. */
        //    return this.minDirection.Copy();
        //}

        //private void MoveOld()
        //{
        //    Vector newDirection = this.SimulateNextDirection();
        //    this.direction = newDirection.Copy();
        //    this.UpdateWall();
        //}

        /* Decide si el flipper ha llegado a su posición
         * límite superior. */
        //private bool ReachedUpperLimit()
        //{
        //    if (this.situation == "left")
        //    {
        //        return this.direction.GetSlope() > this.maxDirection.GetSlope();
        //    }
        //    else
        //    {
        //        return this.direction.GetSlope() < this.maxDirection.GetSlope();
        //    }
        //}

        /* Decide si el flipper ha llegado a su posición 
         * límite inferior. */
        //private bool ReachedLowerLimit()
        //{
        //    if (this.situation == "left")
        //    {
        //        return this.direction.GetSlope() < this.minDirection.GetSlope();
        //    }
        //    else
        //    {
        //        return this.direction.GetSlope() > this.minDirection.GetSlope();
        //    }
        //}

        /* Rota el flipper hacia arriba con el valor del ángulo. */
        //private Vector RotateUp()
        //{
        //    if (this.situation == "left")
        //    {
        //        return this.direction.NewPositiveRotation(this.angle);
        //    }
        //    else
        //    {
        //        return this.direction.NewNegativeRotation(this.angle);
        //    }
        //}

        /* Rota el flipper hacia abajo con el valor del ángulo. */
        //private Vector RotateDown()
        //{
        //    if (this.situation == "left")
        //    {
        //        return this.direction.NewNegativeRotation(this.angle);
        //    }
        //    else
        //    {
        //        return this.direction.NewPositiveRotation(this.angle);
        //    }
        //}

        

        /* Actualiza la pared antes de comprobar colisión. */
        //private void UpdateWall()
        //{
        //    Vector[] ends;
        //    if (this.situation == "left")
        //    {
        //        ends = new Vector[2]
        //        {
        //            this.position,
        //            Vector.Sum(this.position, this.direction)
        //        };
        //    }
        //    else
        //    {
        //        ends = new Vector[2]
        //        {
        //            Vector.Sum(this.position, this.direction),
        //            this.position
        //        };
        //    }
        //    this.wall = new Wall(ends, this.rad);
        //}

        

        /* DEPRECATED: Código antiguo. */

        /* Mueve el flipper, si se tiene que mover. Para ello simula su nueva posición
         * y comprueba que no colisiona con la pelota. */
        //public void MoveOld(Ball ball)
        //{
        //    if (!this.movingUp && !this.movingDown)
        //    {
        //        /* El flipper no se está moviendo. */
        //        Console.WriteLine("flipper no se mueve");
        //        return;
        //    }

        //    /* Hacemos la simulación del movimiento del flipper y calculamos su pared. */
        //    Vector newDirection = this.SimulateNextDirection();
        //    Wall newWall = Flipper.CalculateWall(this.position, newDirection, this.situation, this.rad);
        //    //Wall wall = new Wall(ends, this.rad);

        //    if (newWall.CheckCollision(ball))
        //    {
        //        /* El flipper movido va a chocar contra la pelota,
        //         * así que no movemos nada. */
        //        return;
        //    }

        //    /* El nuevo flipper no colisiona con la pelota, movemos
        //     * y actualizamos la pared. */
        //    this.direction = newDirection.Copy();
        //    this.wall = Flipper.CalculateWall(this.position, this.direction, this.situation, this.rad);

        //    /* Actualizamos los sentidos de movimiento. */
        //    if (this.movingUp)
        //    {
        //        if (this.ReachedUpperLimit())
        //        {
        //            this.movingUp = false;
        //            this.movingDown = true;
        //            return;
        //        }
        //    }
        //    else if (this.movingDown)
        //    {
        //        if (this.ReachedLowerLimit())
        //        {
        //            this.movingUp = false;
        //            this.movingDown = false;
        //            return;
        //        }
        //    }
        //}

        /* Decide si está colisionando con una bola. */
        //public bool CheckCollision(Ball ball)
        //{
        //    /* Si la bola está muy por encima del flipper, directamente no comprobamos. */
        //    if (ball.Position.Y > this.position.Y + this.length)
        //    {
        //        return false;
        //    }

        //    /* Actualizo la pared del flipper. */
        //    this.UpdateWall();

        //    /* Compruebo colisión con la pared actualizada. */
        //    return this.wall.CheckCollision(ball);
        //}

        ///* Aplica la colisión a una pelota. */
        //public void ApplyCollisionOld(Ball ball)
        //{
        //    /* TODO: esta función será más compleja en el futuro. */
        //    this.wall.ApplyCollision(ball);
        //}
    }
}
