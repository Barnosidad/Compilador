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
        // Me guardo el primer token en la bolsa, sin importar que sea pero debe ser global para todos dentro de una produccion
        public Lenguaje()
        {
            IndentCont = 0;
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            IndentCont = 0;
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
            if (Clasificacion == Tipos.SNT)
            {
                lenguajecs.WriteLine(IndentString() + "public void " + Contenido + "()");
                lenguajecs.WriteLine(IndentString() + "{");
                IndentCont++;
            }
            match(Tipos.SNT);
            match(Tipos.Flecha);
            // Me conviene guardar todo el contenido y imprimir al final, o bien hacer la lista de todo y imprimir la misma lista al final, con todo y lo de las recursividades
            // Podria almacenar todo en la misma cola, pero los parentesis crearian algunos conflictos, habria que probar posibilidades
            List<TokensOpt> listaTokens = new List<TokensOpt>();
            Stack<Control> stackControl = new Stack<Control>();
            ConjuntoTokens(ref listaTokens, ref stackControl);
            stackControl.ToList().ForEach(x => Console.WriteLine(x));
            listaTokens.ForEach(x => Console.WriteLine(x.TClasificacion + ", " + x.TContenido + ", " + x.TGOrder + ", " + x.TLOrder));
            match(Tipos.FinProduccion);
            // Ahora con todas estas listas, toca trabajar en la impresion y debe ser simplista
            servicioDeImpresion(listaTokens, stackControl);
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
            if (Clasificacion == Tipos.SNT)
            {
                Producciones();
            }
        }
        private void ConjuntoTokens(ref List<TokensOpt> listaTokens, ref Stack<Control> stackControl)
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
                // La lista compartida estaria aqui, todo se imprimiria a posteriori y lo que empieza acaba aqui
                conjuntoDelParentesis(ref listaTokens, ref stackControl, 0);
                // la impresion viene hasta el primer parentesis izquiero y despues
                // empieza la siguiente hasta el derecho, la parte inicial se separa del cuerpo
                // que dentro puede tener otro cuerpo
            }
            else if(Clasificacion != Tipos.FinProduccion) throw new Error("TIPO NO PERMITIDO", log, linea);
            if (Clasificacion != Tipos.FinProduccion)
            {
                ConjuntoTokens(ref listaTokens, ref stackControl);
            }
        }
        // Pasar la cola de tokens para imprimir en orden, tal vez use un "servicio de impresion"
        private void conjuntoDelParentesis(ref List<TokensOpt> listaTokens, ref Stack<Control> listaControl, int order)
        {
            int localGOrder = order;
            int localOrder = 0;
            match(Tipos.PIzquierdo);
            log.WriteLine("------ Almacenaje de tokens en parentesis ------");
            while(Clasificacion != Tipos.PDerecho)
            {
                if(Clasificacion == Tipos.ST)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localGOrder, localOrder));
                    localOrder++;
                    match(Tipos.ST);
                }
                else if(Clasificacion == Tipos.SNT)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localGOrder, localOrder));
                    localOrder++;
                    match(Tipos.SNT);
                }
                else if(Clasificacion == Tipos.Tipo)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localGOrder, localOrder));
                    localOrder++;
                    match(Tipos.Tipo);
                }
                else if(Clasificacion == Tipos.Or)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, "", localGOrder));
                    match(Tipos.Or);
                }
                else if(Clasificacion == Tipos.PIzquierdo)
                {
                    // No permite un parentesis dentro de otro parentesis
                    // throw new Error("TIPO NO PERMITIDO", log, linea);
                    // Intentemos hacerlo
                    conjuntoDelParentesis(ref listaTokens, ref listaControl, ++order);
                }
            }
            log.WriteLine("------ Fin Almacenaje de tokens en parentesis ------");
            match(Tipos.PDerecho);
            // Post almacenaje se hacen las comprobaciones y generacion de codigo
            if(Clasificacion == Tipos.Epsilon)
            {
                // Si en el mismo orden hay suficientes ORS salta error
                if(listaTokens.FindAll(x => x.TGOrder == localGOrder).Any(x => x.TClasificacion == Tipos.Or)) throw new Error("No puede haber OR junto con EPSILON", log, linea);
                // Es obligatorio especificar la condicion de recurisivdad
                if(listaTokens.FindAll(x => x.TGOrder == localGOrder).First().TClasificacion == Tipos.SNT) throw new Error("ES NECESARIO ESPECIFICAR LA CONDICION DE RECURSIVIDAD ST O TIPOS", log, linea);
                listaControl.Push(Control.Recursivo);
                match(Tipos.Epsilon);
            }
            else
            {
                // Si en el mismo orden hay suficientes ORS
                if(listaTokens.FindAll(x => x.TGOrder == localGOrder).Any(x => x.TClasificacion == Tipos.Or))
                {
                    // Veo si estan en orden los ORS
                    for(int i = 0; i < listaTokens.FindAll(x => x.TGOrder == localGOrder).Count; i++) if(i % 2 != 0) if(listaTokens.FindAll(x => x.TGOrder == localGOrder).ElementAt(i).TClasificacion != Tipos.Or) throw new Error("El OR no esta en orden",log,linea);
                    // Veo si son ST o TIPO
                    for(int i = 0; i < (listaTokens.FindAll(x => x.TGOrder == localGOrder).Count - 1); i++) if(listaTokens.FindAll(x => x.TGOrder == localGOrder).ElementAt(i).TClasificacion == Tipos.SNT) throw new Error("NO PUDES USAR SNT EN LOS IF SECUENCIALES",log,linea);
                    // Elimino los OR
                    listaTokens.FindAll(x => x.TGOrder == localGOrder).RemoveAll(x => x.TClasificacion == Tipos.Or);
                    listaControl.Push(Control.Secuencial);
                } 
                else 
                {
                    listaControl.Push(Control.Normal);
                }
            }
        }
        // Para las condiciones solo se imprimen las "Tapas"
        private void servicioDeImpresion(List<TokensOpt> listaTokens, Stack<Control> stackControl)
        {
            foreach (TokensOpt t in listaTokens)
            {
                // Primero debo de ver que tipo de control es
                Console.WriteLine("Elemento en " + t.TGOrder + ": " + stackControl.ToList().ElementAt(t.TGOrder));
                if(stackControl.ToList().ElementAt(t.TGOrder) == Control.Normal)
                {
                    if (t.TClasificacion == Tipos.SNT) lenguajecs.WriteLine(IndentString() + t.TContenido + "();");
                    else if (t.TClasificacion == Tipos.ST) lenguajecs.WriteLine(IndentString() + "match(\"" + t.TContenido + "\");");
                    else if (t.TClasificacion == Tipos.Tipo) lenguajecs.WriteLine(IndentString() + "match(Tipos." + t.TContenido + ");");
                }
                if(stackControl.ToList().ElementAt(t.TGOrder) == Control.Recursivo)
                {
                    if(t.TLOrder == 0)
                    {
                        lenguajecs.Write(IndentString() + "if (");
                        if (t.TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine("Contenido == \"" + t.TContenido + "\")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            // lenguajecs.WriteLine(IndentString() + "match(\"" + Cola.First().TContenido + "\");");
                        }
                        else if (t.TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine("Clasificacion == Tipos." + t.TContenido + ")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            // lenguajecs.WriteLine(IndentString() + "match(Tipos." + Cola.First().TContenido + ");");
                        }
                    }
                    else
                    {
                        if (t.TClasificacion == Tipos.SNT)
                        {
                            lenguajecs.WriteLine(IndentString() + t.TContenido + "();");
                        }
                        else if (t.TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(\"" + t.TContenido + "\");");
                        }
                        else if (t.TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + t.TContenido + ");");
                        }
                    }
                }
            }
        }

        private struct TokensOpt
        {
            public TokensOpt(Tipos _clasificacion, string _contenido = "", int _globalOrder = 0, int _localOrder = 0)
            {
                _TClasificacion = _clasificacion;
                _TContenido = _contenido;
                _TGOrder = _globalOrder;
                _TLOrder = _localOrder;
            }
            private int _TGOrder;
            private int _TLOrder;
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
            public int TGOrder
            {
                get => _TGOrder;
            }
            public int TLOrder
            {
                get => _TLOrder;
            }
        }
        private enum Control
        {
            Recursivo, Secuencial, Normal
        }
    }
}