using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;

namespace pinball_v3
{
    /* Estructura de variables generales que se le pasa al constructor
     * de los flippers. */
    public struct FlipperData
    {
        public readonly int length;
        public readonly int height;
        public double interval;

        /* Constructor. */
        public FlipperData(int length, int height, double interval)
        {
            this.length = length;
            this.height = height;
            this.interval = interval;
        }
    }

    public partial class Form1 : Form
    {
        /* Tamaños del canvas. */
        private readonly int canvasWidth = 500;
        private readonly int canvasHeight = 600;

        /* Seed para números aleatrorios. */
        private readonly Random rand = new Random();

        /* Variables físicas. */
        private readonly double gravity = 80;
        private readonly double interval = 0.04;
        private readonly double friction = 0.6;

        /* Flippers. */
        private readonly int flipperLength = 100;
        private readonly int flipperHeight = 10;
        private readonly FlipperData flipperData;
        private List<Flipper> flippers;

        /* La bola. */
        private Ball ball;
        private readonly int radius = 8;

        /* Bumpers. */
        private readonly int bumperRad = 20;
        private List<Bumper> bumpers;

        /* Función constructora de la form. */
        public Form1()
        {
            InitializeComponent();

            /* Añadimos el timer y lo asociamos a la función OnTimedEvent. */
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1;
            aTimer.Enabled = true;

            /* Funciones de callback para eventos. */
            Paint += new PaintEventHandler(Form1_Paint);
            KeyDown += new KeyEventHandler(Form1_KeyDown);
            //KeyUp += new KeyEventHandler(Form1_KeyUp);

            KeyPreview = true;
            DoubleBuffered = true;

            /* Iniciamos algunas variables para crear los flippers. */
            this.flippers = new List<Flipper>(2);
            Vector flipper0position = new Vector(this.canvasWidth / 4, this.canvasHeight / 5);
            Vector flipper1position = new Vector(3 * this.canvasWidth / 4, this.canvasHeight / 5);
            this.flipperData = new FlipperData(this.flipperLength, this.flipperHeight, this.interval);

            this.flippers.Add(new Flipper("left", flipper0position, -Math.PI / 5, flipperData));
            this.flippers.Add(new Flipper("right", flipper1position, Math.PI + Math.PI / 5, flipperData));

            /* Creamos los bumpers. */
            Vector[] bumperPositions = new Vector[3]
            {
                new Vector(this.canvasWidth / 4, 3 * this.canvasHeight / 4),
                new Vector(this.canvasWidth / 2, 3 * this.canvasHeight / 4),
                new Vector(3 * this.canvasWidth / 4, 3 * this.canvasHeight / 4)
            };
            this.bumpers = new List<Bumper>(3);
            for (int i = 0; i < 3; i++)
            {
                this.bumpers.Add(new Bumper(bumperPositions[i], this.bumperRad));
            }

            /* Creamos la pelota. */
            Vector ballPosition = new Vector(this.canvasWidth / 2, this.canvasHeight - 100);
            Vector ballVelocity = new Vector(rand.Next(1, 250), rand.Next(1, 250));
            this.ball = new Ball(ballPosition, ballVelocity, this.radius);
        }

        /* Función de callback para cada click del timer. */
        private void OnTimedEvent(object sender, EventArgs e)
        {
            /* Movemos los elementos. */
            this.ball.Move();
            for (int i = 0; i < this.flippers.Count; i++)
            {
                this.flippers[i].UpdateFlipperAngle();
            }

            /* Comprobar la colisión de la bola con los bordes del canvas. */
            if (this.ball.CheckCanvas(canvasWidth, canvasHeight))
            {
                /* Ha habido colisión con las paredes del canvas. */
                Invalidate();
                return;
            }

            /* Comprobamos una primera colisión "naive" con los flippers. */
            for (int i = 0; i < this.flippers.Count; i++)
            {
                if (this.flippers[i].HandleCollisionSAT(this.ball))
                {
                    Console.WriteLine("Colisión con flipper");
                    Invalidate();
                    return;
                }
            }

            /* Comprobar colisión con cada uno de los bumpers. */
            for (int i = 0; i < this.bumpers.Count; i++)
            {
                if (this.bumpers[i].HandleCollision(this.ball, friction))
                {
                    Console.WriteLine("Colisión con bumper");
                    Invalidate();
                    return;
                }
            }


            /* Si no ha habido ninguna colisión, comprobemos que no nos hemos
             * pasado una colisión con flippers (ahora aplicaremos ray tracing). */
            //for (int i = 0; i < this.flippers.Count; i++)
            //{
            //    if (this.flippers[i].HandleCollisionRayTracing(this.ball))
            //    {
            //        Console.WriteLine("Colisión con flippers detectada mediante ray tracing");
            //        Invalidate();
            //        return;
            //    }
            //}
        }

        /* Función que gestiona el evento de dibujar en pantalla. */
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            /* Dibujar los flippers. */
            for (int i = 0; i < this.flippers.Count; i++)
            {
                
                this.flippers[i].Draw(e.Graphics, this.canvasHeight);
            }

            /* Dibujar la pelota. */
            this.ball.Draw(e.Graphics, this.canvasHeight);

            /* Dibujar los bumpers. */
            for (int k = 0; k < this.bumpers.Count; k++)
            {
                this.bumpers[k].Draw(e.Graphics, this.canvasHeight);
            }

            Invalidate();
        }

        /* Función de callback para cuando se deja de pulsar una tecla. */
        //private void Form1_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Left)
        //    {
        //        /* Indicar al flipper de la izquierda que la tecla
        //         * se ha dejado de pulsar. */
        //        this.flippers[0].KeyReleased();
        //    }
        //    else if (e.KeyCode == Keys.Right)
        //    {
        //        /* Indicar al flipper de la derecha que la tecla
        //         * se ha dejado de pulsar. */
        //        this.flippers[1].KeyReleased();
        //    }
        //}

        /* Función de callback para cuando se presiona una tecla. */
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                /* Indicar que el flipper izquierdo debe rotar hacia arriba. */
                this.flippers[0].KeyPressed();
            }
            else if (e.KeyCode == Keys.Right)
            {
                /* Empezar a mover el flipper de la derecha. */
                this.flippers[1].KeyPressed();
            }
        }







        /* La parte gráfica está basada en objetos de tipo polígono. */
        //List<Polygon> polygons = new List<Polygon>();

        ////Polygon wall1;
        ////Polygon wall2;
        ////Polygon wall3;
        ////Polygon wall4;
        //Polygon bumper;
        //Polygon bumper2;
        ////Polygon marker;
        //Polygon flipper1;
        //Polygon flipper2;
        ////Polygon bola;
        //Vector Flipper1_Centro = new Vector(0, 420);
        //Vector Flipper2_Centro = new Vector(100, 420);

        //Ball ball;

        ///* Tamaño del canvas. */
        //int width = 800;
        //int height = 1000;

        //float angulo = 45; // ????

        /* Función constructora de la form. */
        //public Form1()
        //{
        //    /* Iniciamos componente por defecto. */
        //    InitializeComponent();

        //    /* Añadimos el timer y lo asociamos a la función OnTimedEvent. */
        //    System.Timers.Timer aTimer = new System.Timers.Timer();
        //    aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        //    aTimer.Interval = 1;
        //    aTimer.Enabled = true;

        //    /* Eventos para pintar y presionar o soltar teclas. */
        //    Paint += new PaintEventHandler(Form1_Paint);
        //    KeyDown += new KeyEventHandler(Form1_KeyDown);
        //    KeyUp += new KeyEventHandler(Form1_KeyUp);

        //    KeyPreview = true;
        //    DoubleBuffered = true;

        //    /* Creamos la pelota. */
        //    Vector ballPos = new Vector(this.width / 2, 100);
        //    Vector ballVel = new Vector(5, 5);
        //    this.ball = new Ball(ballPos, ballVel, 25);

        //    /* Fabricamos los polígonos que va a haber en el canvas. */

        //    // Esta es la bola.
        //    //Polygon p = new Polygon();
        //    //p.Vertices.Add(new Vector(0, 0));
        //    //p.Vertices.Add(new Vector(20, 0));
        //    //p.Vertices.Add(new Vector(20, 20));
        //    //p.Vertices.Add(new Vector(0, 20));
        //    //p.Move(100, 10);
        //    //poligonos.Add(p);

        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(0, 0));
        //    p.Vertices.Add(new Vector(0, 780));
        //    poligonos.Add(p);

        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(0, 780));
        //    p.Vertices.Add(new Vector(640, 780));
        //    poligonos.Add(p);

        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(640, 0));
        //    p.Vertices.Add(new Vector(640, 780));
        //    poligonos.Add(p);

        //    //BUMPER
        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(50, 50));
        //    p.Vertices.Add(new Vector(100, 0));
        //    p.Vertices.Add(new Vector(150, 150));
        //    p.Move(200, 250);

        //    poligonos.Add(p);

        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(0, 50));
        //    p.Vertices.Add(new Vector(50, 0));
        //    p.Vertices.Add(new Vector(60, 80));
        //    p.Move(200, 100);

        //    poligonos.Add(p);

        //    p = new Polygon();
        //    p.Vertices.Add(new Vector(0, 0));
        //    p.Vertices.Add(new Vector(640, 0));
        //    poligonos.Add(p);

        //    InitFlipper1(true);
        //    InitFlipper2(true);

        //    /* Construimos los lados de los polígonos que hemos definido. */
        //    foreach (Polygon Poligono in poligonos) Poligono.UpdateEdges();

        //    bola = poligonos[0];

        //    wall1 = poligonos[1];
        //    wall2 = poligonos[2];
        //    wall3 = poligonos[3];
        //    bumper = poligonos[4];
        //    bumper2 = poligonos[5];
        //    wall4 = poligonos[6];

        //    foreach (Polygon poligono in poligonos) poligono.UpdateEdges();

        //    bola = poligonos[0];
        //}



        //  Estructura que almacena los resultados de la funcion ColisionPoligono
        //public struct ColisionPoligonoResult
        //{
        //    public bool Intersectara; // ¿Los polígonos intersectarán en el futuro?
        //    public bool Intersecta; // ¿Los polígonos están intersectando en este momento?
        //    public Vector VectorTraslacionMinimo; // La traslacion se aplica al polígono A para empujar los polígonos .
        //    public float punto_impacto;
        //    public Vector sentido;
        //}

        // Comprobar si el polígono A va a colisionar con el polígono B a una determinada velocidad
        //public ColisionPoligonoResult ColisionPoligono(Polygon poligonoA, Polygon poligonoB, Vector velocidad)
        //{
        //    ColisionPoligonoResult result = new ColisionPoligonoResult();
        //    result.Intersecta = true;
        //    result.Intersectara = true;

        //    int NumeroLadosA = poligonoA.Lados.Count;
        //    int NumeroLadosB = poligonoB.Lados.Count;
        //    float minIntervalo_Distancia = float.PositiveInfinity;
        //    Vector EjeTraslacion = new Vector();
        //    Vector lado;

        //    // Bucle a través de todos los lados de todos los polígonos
        //    for (int ladoIndex = 0; ladoIndex < NumeroLadosA + NumeroLadosB; ladoIndex++)
        //    {
        //        if (ladoIndex < NumeroLadosA)
        //        {
        //            lado = poligonoA.Lados[ladoIndex];
        //        }
        //        else
        //        {
        //            lado = poligonoB.Lados[ladoIndex - NumeroLadosA];
        //        }

        //        // ===== 1. Encontrar si los polígonos se intersectan en este momento =====

        //        // Encontrar el eje perpendicular al lado actual
        //        Vector axis = new Vector(-lado.Y, lado.X);
        //        axis.Normalizar();

        //        // Encontrar la proyección del polígono del eje actual
        //        float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
        //        ProyectarPoligono(axis, poligonoA, ref minA, ref maxA);
        //        ProyectarPoligono(axis, poligonoB, ref minB, ref maxB);

        //        // Comprobar su las proyecciones de los polígonos se intersectan en este momento
        //        if (Intervalo_Distancia(minA, maxA, minB, maxB) > 0) result.Intersecta = false;

        //        // ===== 2. Ahora buscar si los polígono se intersectarán en el futuro =====

        //        // Proyectar la velocidad del eje actual
        //        float ProyectarVelocidad = axis.ProductoPunto(velocidad);

        //        // Obtener la proyección del polígono A durante el movimiento
        //        if (ProyectarVelocidad < 0)
        //        {
        //            minA += ProyectarVelocidad;
        //        }
        //        else
        //        {
        //            maxA += ProyectarVelocidad;
        //        }

        //        // Realizar la misma comprobación de antes para la nueva proyección
        //        float IntervaloDistancia = Intervalo_Distancia(minA, maxA, minB, maxB);
        //        if (IntervaloDistancia > 0) result.Intersectara = false;

        //        // Si los polígonos no se están intersectando y no se van a intersectar, salimos del bucle
        //        if (!result.Intersecta && !result.Intersectara) break;

        //        // Comprobar si la distancia del intervalo actual es el mínimo. Si no es así,  almacenamos 
        //        // la distancia del intervalo y la distancia actual.
        //        // Esto se usará para calcular el vector de traslación mínimo
        //        IntervaloDistancia = Math.Abs(IntervaloDistancia);
        //        if (IntervaloDistancia < minIntervalo_Distancia)
        //        {
        //            minIntervalo_Distancia = IntervaloDistancia;
        //            EjeTraslacion = axis;

        //            Vector d = poligonoA.Centro - poligonoB.Centro;

        //            result.punto_impacto = ((poligonoA.Centro.X - poligonoB.izquierda.X) / (poligonoB.derecha.X - poligonoB.izquierda.X));


        //            //  distancia = d.X;
        //            if (d.ProductoPunto(EjeTraslacion) < 0) EjeTraslacion = -EjeTraslacion;
        //        }
        //    }

        //    // El vector mínimo de traslación puede ser usado para empujar los polígonos
        //    // Primero movemos los polígonos en función de su velocidad
        //    // luego movemos el poligonoA en función del VectorTraslacionMinimo.
        //    if (result.Intersectara)
        //    {



        //        if (poligonoA.Centro.X < poligonoB.Centro.X) result.sentido.X = -1;
        //        else result.sentido.X = 1;
        //        if (poligonoA.Centro.Y < poligonoB.Centro.Y) result.sentido.Y = -1;
        //        else result.sentido.Y = 1;

        //        result.VectorTraslacionMinimo = EjeTraslacion * minIntervalo_Distancia;
        //    }

        //    return result;
        //}

        // Calculamos la distancia entre [minA, maxA] y [minB, maxB]
        // La distancia será negativa si los intervalos se solapan
        //public float Intervalo_Distancia(float minA, float maxA, float minB, float maxB)
        //{
        //    if (minA < minB)
        //    {
        //        return minB - maxA;
        //    }
        //    else
        //    {
        //        return minA - maxB;
        //    }
        //}

        // Calculamos la proyección de un polígono en un eje y lo  devuelve como un intervalo [min, max]
        //public void ProyectarPoligono(Vector axis, Polygon poligono, ref float min, ref float max)
        //{
        //    // Proyección de un punto en un eje
        //    float d = axis.ProductoPunto(poligono.Puntos[0]);
        //    min = d;
        //    max = d;
        //    for (int i = 0; i < poligono.Puntos.Count; i++)
        //    {
        //        d = poligono.Puntos[i].ProductoPunto(axis);
        //        if (d < min)
        //        {
        //            min = d;
        //        }
        //        else
        //        {
        //            if (d > max)
        //            {
        //                max = d;
        //            }
        //        }
        //    }
        //}





        //Vector velocidad = new Vector(0, 0);
        //Vector gravedad = new Vector(0, 0.09f);
        //Vector rebote = new Vector(-0.5f, -0.5f);
        //Vector toca = new Vector(0, 0.5f);



        //public void InitFlipper1(bool init)
        //{

        //    if (init == true) flipper1 = new Polygon();
        //    else flipper1.Puntos.Clear();

        //    flipper1.Puntos.Add(new Vector(0, 0));
        //    flipper1.Puntos.Add(new Vector(200, 0));
        //    flipper1.Puntos.Add(new Vector(200, 20));
        //    flipper1.Puntos.Add(new Vector(0, 20));
        //    flipper1.Desplazamiento(100, 600);

        //    if (init == true) poligonos.Add(flipper1);

        //}



        //public void InitFlipper2(bool init)
        //{

        //    if (init == true) flipper2 = new Polygon();
        //    else flipper2.Puntos.Clear();

        //    flipper2.Puntos.Add(new Vector(200, 0));
        //    flipper2.Puntos.Add(new Vector(200, 20));
        //    flipper2.Puntos.Add(new Vector(0, 20));
        //    flipper2.Puntos.Add(new Vector(0, 0));

        //    flipper2.Desplazamiento(400, 600);

        //    if (init == true) poligonos.Add(flipper2);

        //}

        //public void RotarPoligono(Polygon poligono, Vector centro, float angulo)
        //{



        //    if (poligono == flipper1) InitFlipper1(false);
        //    if (poligono == flipper2) InitFlipper2(false);

        //    for (int punt = 0; punt < poligono.Puntos.Count; punt++)
        //    {
        //        poligono.Puntos[punt] = RotatePoint(poligono.Puntos[punt], centro, angulo);
        //    }



        //    poligono.ConstruirLados();


        //    //   poligonos[poligonos.FindIndex(x => x == poligono)] = poligono;

        //}


        //ColisionPoligonoResult r;
        //float ultimo_angulo;
        /* Función de callback para cada tick del reloj. */
        //private void OnTimedEvent(object sender, ElapsedEventArgs e)
        //{

        //    velocidad += gravedad;
        //    Vector traslacionBola = velocidad;

        //    ultimo_angulo = angulo;

        //    if (flipper_pressed1 == true)
        //    {
        //        if (angulo > -45) angulo = angulo - 10f;
        //    }
        //    else
        //    {
        //        if (angulo < 45) angulo = angulo + 10f;
        //    }

        //    RotarPoligono(flipper1, flipper1.Puntos[0], angulo);
        //    RotarPoligono(flipper2, flipper2.Puntos[0], -angulo);


        //    foreach (Polygon poligono in poligonos)
        //    {
        //        if (poligono == bola) continue;

        //        r = ColisionPoligono(bola, poligono, velocidad);

        //        if (r.Intersectara || r.Intersecta)
        //        {
        //            //velocidad.X = 2 * r.VectorTraslacionMinimo.X;
        //            //velocidad.Y *= -rebote.Y;
        //            //traslacionBola = velocidad + r.VectorTraslacionMinimo;


        //            if (poligono == wall1 || poligono == wall2 || poligono == wall3 || poligono == wall4)
        //            {
        //                velocidad.X = 2 * r.VectorTraslacionMinimo.X;
        //                velocidad.Y *= rebote.Y / 2;
        //                traslacionBola = velocidad + r.VectorTraslacionMinimo;


        //            }



        //            if (poligono == bumper || poligono == bumper2)
        //            {
        //                velocidad.X = -10 * rebote.X * r.VectorTraslacionMinimo.X;
        //                velocidad.Y *= -rebote.Y;
        //                traslacionBola = velocidad + r.VectorTraslacionMinimo;

        //            }


        //            if (poligono == flipper1)
        //            {
        //                //float vel = (angulo - ultimo_angulo)/2;
        //                //Debug.WriteLine("ha tocado el filpper " + r.punto_impacto + " **** " + vel + "  ++++ " + r.VectorTraslacionMinimo.Y);

        //                //if (flipper_pressed1 == true)
        //                //{
        //                //    velocidad.X *= 2 * r.VectorTraslacionMinimo.X;
        //                //    velocidad.Y *= -rebote.Y;
        //                //}

        //                //   velocidad += new Vector(0, -10 * ( r.punto_impacto)*(vel));
        //                //if (vel <= 0) vel = 1;
        //                //if (r.VectorTraslacionMinimo.Y < -10) r.VectorTraslacionMinimo.Y = -10;
        //                //velocidad.X *= rebote.X  + r.VectorTraslacionMinimo.X;
        //                //velocidad.Y *= rebote.Y  + r.VectorTraslacionMinimo.Y/10;
        //                //  traslacionBola = velocidad;


        //                velocidad.X = r.punto_impacto + rebote.X * r.VectorTraslacionMinimo.X;
        //                velocidad.Y = 1.3f * r.VectorTraslacionMinimo.Y;
        //                traslacionBola = velocidad;




        //            }


        //            if (poligono == flipper2)
        //            {
        //                float vel = (angulo - ultimo_angulo) / 5;
        //                if (vel <= 0) vel = 1;
        //                Debug.WriteLine("ha tocado el filpper2 punto impacto X: " + r.punto_impacto + " rebote X " + rebote.X + "  TRASLACION X " + r.VectorTraslacionMinimo.X + " vel: " + vel);
        //                Debug.WriteLine("ha tocado el filpper2 punto impacto Y: " + r.punto_impacto + " rebote Y " + rebote.Y + " TRASLACION Y " + r.VectorTraslacionMinimo.Y + " vel: " + vel);


        //                velocidad.X = vel + r.VectorTraslacionMinimo.X;
        //                velocidad.Y = vel + r.VectorTraslacionMinimo.Y;


        //                traslacionBola = velocidad;





        //            }


        //            break;
        //        }
        //        else
        //        {
        //            traslacionBola = velocidad;

        //        }
        //    }

        //    bola.Desplazamiento(traslacionBola);
        //}


        /* Función de callback para pintar sobre el canvas. */
        //void Form1_Paint(object sender, PaintEventArgs e)
        //{
        //    Vector p1;
        //    Vector p2;
        //    foreach (Polygon poligono in poligonos)
        //    {
        //        for (int i = 0; i < poligono.Puntos.Count; i++)
        //        {
        //            p1 = poligono.Puntos[i];
        //            if (i + 1 >= poligono.Puntos.Count)
        //            {
        //                p2 = poligono.Puntos[0];
        //            }
        //            else
        //            {
        //                p2 = poligono.Puntos[i + 1];
        //            }
        //            e.Graphics.DrawLine(new Pen(Color.Black), p1, p2);
        //        }
        //    }

        //    Invalidate();
        //}





        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        //public Vector RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        //{
        //    double angleInRadians = angleInDegrees * (Math.PI / 180);
        //    double cosTheta = Math.Cos(angleInRadians);
        //    double sinTheta = Math.Sin(angleInRadians);
        //    return new Vector
        //    {
        //        X =
        //            (int)
        //            (cosTheta * (pointToRotate.X - centerPoint.X) -
        //            sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
        //        Y =
        //            (int)
        //            (sinTheta * (pointToRotate.X - centerPoint.X) +
        //            cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
        //    };

        //}

        //bool flipper_pressed1;

        /* Función de callback al presionar una tecla. */
        //void Form1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    Debug.WriteLine(e.KeyValue);
        //    Vector velocidad = new Vector();
        //    flipper_pressed1 = false;
        //    switch (e.KeyValue)
        //    {


        //        case 13: //RETURN

        //            flipper_pressed1 = true;


        //            break;



        //        case 49: //1

        //            bola.Puntos.Clear();
        //            bola.Puntos.Add(new Vector(0, 0));
        //            bola.Puntos.Add(new Vector(20, 0));
        //            bola.Puntos.Add(new Vector(20, 20));
        //            bola.Puntos.Add(new Vector(0, 20));
        //            bola.Desplazamiento(480, 50);





        //            break;

        //        case 32: //espacio

        //            bola.Puntos.Clear();
        //            bola.Puntos.Add(new Vector(0, 0));
        //            bola.Puntos.Add(new Vector(20, 0));
        //            bola.Puntos.Add(new Vector(20, 20));
        //            bola.Puntos.Add(new Vector(0, 20));
        //            bola.Desplazamiento(320, 50);





        //            break;

        //        case 38: // arriba

        //            Vector hit = new Vector(10, 30);
        //            velocidad += hit;
        //            break;

        //        case 40: // abajo


        //            velocidad = new Vector(0, -10);

        //            break;

        //        case 39: // derecha

        //            hit = new Vector(-30, 0);
        //            velocidad += hit;
        //            break;



        //        case 37: // izquierda


        //            hit = new Vector(30, 0);
        //            velocidad += hit;
        //            break;
        //    }

        //Vector traslacionBola = velocidad;

        //foreach (Polygon poligono in poligonos) {
        //    if (poligono == bola) continue;

        //    ColisionPoligonoResult r = ColisionPoligono(bola, poligono, velocidad);

        //    if (r.Intersectara) {
        //        traslacionBola = velocidad + r.VectorTraslacionMinimo;
        //        break;
        //    }


        //    if (r.Intersecta)
        //    {
        //        traslacionBola = velocidad + r.VectorTraslacionMinimo;
        //        break;
        //    }
        //}

        //bola.Desplazamiento(traslacionBola);
    }
}

