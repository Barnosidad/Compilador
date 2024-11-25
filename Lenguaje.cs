using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualBasic;

/*
    Requerimiento 1: Solo la primera produccion es publica, las demas son privadas
    Requerimiento 2: Implementar la cerradura epsilon
    Requerimiento 3: Imprementar el operador OR 
    Requerimiento 4: Indentar el codigo (Aumentar de manera dinamica los tabuladores con los corchetes)
    Conjunto de tokens, listas de recursividad con el mismo objeto, lista de epsilon ?
    Si viene or, ni checo epsilon, si no viene or solo puede venir epsilon teniendo en cuenta
    los parentesis.
    Al momento de hacer esto, que es lo de describe la recursividad o bien lo que define lo optativo
*/

namespace Compilador
{
    public class Lenguaje : Sintaxis
    {
        // Tal vez necesite una pila para almacenar las opcionales y el or
        private int IndentCont;
        protected bool PrimerProduccion;
        public Lenguaje()
        {
            IndentCont = 0;
            PrimerProduccion = true;
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            IndentCont = 0;
            PrimerProduccion = true;
        }
        private string IndentString()
        {
            string IndentLength = "";
            for (int i = 0; i < IndentCont; i++)
            {
                IndentLength += "\t";
            }
            return IndentLength;
        }
        private void esqueleto(string nspace)
        {
            lenguajecs.WriteLine(IndentString() + "using System;");
            lenguajecs.WriteLine(IndentString() + "using System.Collections.Generic;");
            lenguajecs.WriteLine(IndentString() + "using System.Linq;");
            lenguajecs.WriteLine(IndentString() + "using System.Net.Http.Headers;");
            lenguajecs.WriteLine(IndentString() + "using System.Reflection.Metadata.Ecma335;");
            lenguajecs.WriteLine(IndentString() + "using System.Runtime.InteropServices;");
            lenguajecs.WriteLine(IndentString() + "using System.Threading.Tasks;");
            lenguajecs.WriteLine(IndentString() + "\nnamespace " + nspace);
            lenguajecs.WriteLine(IndentString() + "{");
            IndentCont++;
            lenguajecs.WriteLine(IndentString() + "public class Lenguaje : Sintaxis");
            lenguajecs.WriteLine(IndentString() + "{");
            IndentCont++;
            lenguajecs.WriteLine(IndentString() + "public Lenguaje()");
            lenguajecs.WriteLine(IndentString() + "{");
            IndentCont++;
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
            lenguajecs.WriteLine(IndentString() + "public Lenguaje(string nombre) : base(nombre)");
            lenguajecs.WriteLine(IndentString() + "{");
            IndentCont++;
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
        }
        public void genera()
        {
            match("namespace");
            match(":");
            esqueleto(Contenido);
            match(Tipos.SNT);
            match(";");
            Producciones();
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
        }
        private void Producciones()
        {
            if (Clasificacion == Tipos.SNT && PrimerProduccion == true)
            {
                lenguajecs.WriteLine(IndentString() + "public void " + Contenido + "()");
                lenguajecs.WriteLine(IndentString() + "{");
                IndentCont++;
                PrimerProduccion = false;
            }
            if (Clasificacion == Tipos.SNT && PrimerProduccion == false)
            {
                lenguajecs.WriteLine(IndentString(IndentCont) + "private void " + Contenido + "()");
                lenguajecs.WriteLine(IndentString(IndentCont) + "{");
                IndentCont++;
            }
            match(Tipos.SNT);
            match(Tipos.Flecha);
            ConjuntoTokens();
            match(Tipos.FinProduccion);
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
            if (Clasificacion == Tipos.SNT)
            {
                Producciones();
            }
        }
        private void ConjuntoTokens()
        {
            // Inicial
            if (Clasificacion == Tipos.SNT)
            {
                lenguajecs.WriteLine(IndentString() + Contenido + "();");
                match(Tipos.SNT);
            }
            else if (Clasificacion == Tipos.ST)
            {
                lenguajecs.WriteLine(IndentString() + "match(\"" + Contenido + "\");");
                match(Tipos.ST);
            }
            else if (Clasificacion == Tipos.Tipo)
            {
                lenguajecs.WriteLine(IndentString() + "match(Tipos." + Contenido + ");");
                match(Tipos.Tipo);
            }
            /*
                ! Al momento de entrar en el parentesis puede haber o no un OR, si lo hay entonces
                ! tiene que haber un if y else if ya que siempre seran dos opciones o mas, asi que la primera pasada revisara
                ! eso y tambien revisara si hay un EPSILON para condicionar todo, el epsilon solo va afuera despues del derecho
                ! una pasada es de reconocimiento y la siguiente es de confirmacion a hacer el procedimiento.
            */
            else if (Clasificacion == Tipos.PIzquierdo)
            {
                // guardo la posicion para regresar, ahora no se si sea necesario regresar
                List<TokensOpt> Cola = new List<TokensOpt>();
                match(Tipos.PIzquierdo);
                log.WriteLine("------ Almacenaje de tokens en parentesis ------");
                while(Clasificacion != Tipos.PDerecho)
                {
                    if(Clasificacion == Tipos.ST)
                    {
                        Cola.Add(new TokensOpt(Clasificacion, Contenido));
                        match(Tipos.ST);
                    }
                    else if(Clasificacion == Tipos.SNT)
                    {
                        Cola.Add(new TokensOpt(Clasificacion, Contenido));
                        match(Tipos.SNT);
                    }
                    else if(Clasificacion == Tipos.Tipo)
                    {
                        Cola.Add(new TokensOpt(Clasificacion, Contenido));
                        match(Tipos.Tipo);
                    }
                    else if(Clasificacion == Tipos.Or)
                    {
                        Cola.Add(new TokensOpt(Clasificacion));
                        match(Tipos.Or);
                    }
                }
                log.WriteLine("------ Fin Almacenaje de tokens en parentesis ------");
                match(Tipos.PDerecho);
                // Post almacenaje se hacen las comprobaciones y generacion de codigo
                if(Clasificacion == Tipos.Epsilon)
                {
                    match(Tipos.Epsilon);
                    if(Cola.Any(x=> x.TClasificacion == Tipos.Or)) throw new Error("No puede haber OR con EPSILON",log,linea);
                    // Generacion de codigo
                    lenguajecs.Write(IndentString() + "if (");
                    if (Cola.First().TClasificacion == Tipos.ST)
                    {
                        lenguajecs.WriteLine("Contenido == \"" + Cola.First().TContenido + "\")");
                        lenguajecs.WriteLine(IndentString() + "{");
                        IndentCont++;
                        lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.First().TContenido + "\");");
                    }
                    else if (Cola.First().TClasificacion == Tipos.Tipo)
                    {
                        lenguajecs.WriteLine("Clasificacion == Tipos." + Cola.First().TContenido + ")");
                        lenguajecs.WriteLine(IndentString() + "{");
                        IndentCont++;
                        lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.First().TContenido + ");");
                    }
                    for(int i = 1; i < Cola.Count; i++)
                    {
                        if (Cola.ElementAt(i).TClasificacion == Tipos.SNT)
                        {
                            lenguajecs.WriteLine(IndentString() + Cola.ElementAt(i).TContenido + "();");
                        }
                        else if (Cola.ElementAt(i).TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.ElementAt(i).TContenido + "\");");
                        }
                        else if (Cola.ElementAt(i).TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.ElementAt(i).TContenido + ");");
                        }
                    }
                    IndentCont--;
                    lenguajecs.WriteLine(IndentString() + "}");
                }
                else
                {
                    // Comprobacion de si hay ORs en orden si es que estos existen
                    if(Cola.Any(x=> x.TClasificacion == Tipos.Or))
                    {
                        for(int i = 0; i < Cola.Count; i++) if(i % 2 != 0) if(Cola.ToList().ElementAt(i).TClasificacion != Tipos.Or) throw new Error("El OR no esta en orden",log,linea);
                        // Remuevo los ors
                        Cola.RemoveAll(x => x.TClasificacion == Tipos.Or);
                        // Generacion de codigo, primer if
                        lenguajecs.Write(IndentString() + "if (");
                        if (Cola.First().TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine("Contenido == \"" + Cola.First().TContenido + "\")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.First().TContenido + "\");");
                        }
                        else if (Cola.First().TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine("Clasificacion == Tipos." + Cola.First().TContenido + ")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.First().TContenido + ");");
                        }
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                        // Los else if de en medio
                        for(int i = 1; i < (Cola.Count - 1); i++)
                        {
                            lenguajecs.Write(IndentString() + "else if (");
                            if (Cola.ElementAt(i).TClasificacion == Tipos.ST)
                            {
                                lenguajecs.WriteLine("Contenido == \"" + Cola.ElementAt(i).TContenido + "\")");
                                lenguajecs.WriteLine(IndentString() + "{");
                                IndentCont++;
                                lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.ElementAt(i).TContenido + "\");");
                            }
                            else if (Cola.ElementAt(i).TClasificacion == Tipos.Tipo)
                            {
                                lenguajecs.WriteLine("Clasificacion == Tipos." + Cola.ElementAt(i).TContenido + ")");
                                lenguajecs.WriteLine(IndentString() + "{");
                                IndentCont++;
                                lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.ElementAt(i).TContenido + ");");
                            }
                            IndentCont--;
                            lenguajecs.WriteLine(IndentString() + "}");
                        }
                        // Ultimo else
                        lenguajecs.WriteLine(IndentString() + "else");
                        lenguajecs.WriteLine(IndentString() + "{");
                        IndentCont++;
                        if (Cola.Last().TClasificacion == Tipos.SNT)
                        {
                            lenguajecs.WriteLine(IndentString() + Cola.Last().TContenido + "();");
                        }
                        else if (Cola.Last().TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.Last().TContenido + "\");");
                        }
                        else if (Cola.Last().TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.Last().TContenido + ");");
                        }
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                    }
                    else
                    {
                        foreach (TokensOpt v in Cola)
                        {
                            if (v.TClasificacion == Tipos.SNT)
                            {
                                lenguajecs.WriteLine(IndentString() + v.TContenido + "();");
                            }
                            else if (v.TClasificacion == Tipos.ST)
                            {
                                lenguajecs.WriteLine(IndentString() + "match(\"" + v.TContenido + "\");");
                            }
                            else if (v.TClasificacion == Tipos.Tipo)
                            {
                                lenguajecs.WriteLine(IndentString() + "match(Tipos." + v.TContenido + ");");
                            }
                        }
                    }
                }
            }
            if (Clasificacion != Tipos.FinProduccion)
            {
                ConjuntoTokens();
            }
        }
        private struct TokensOpt
        {
            public TokensOpt(Tipos _clasificacion, string _contenido = "")
            {
                _TClasificacion = _clasificacion;
                _TContenido = _contenido;
            }
            private string _TContenido;
            private Tipos _TClasificacion;
            public string TContenido
            {
                get => _TContenido;
            }
            public Tipos TClasificacion
            {
                get => _TClasificacion;
            }
        }
    }
}