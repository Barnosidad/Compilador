using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualBasic;

/*
    TRABAJANDO: Requerimiento 1: Solo la primera produccion es publica, las demas son privadas
    HECHO: Requerimiento 2: Implementar la cerradura epsilon
    HECHO: Requerimiento 3: Imprementar el operador OR
    HECHO: Requerimiento 4: Indentar el codigo (Aumentar de manera dinamica los tabuladores con los corchetes)
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
            // No se si aqui aumente el contador global, intentare
            // ! Cada que encuentra un parentesis izquierdo aumenta el contador
            int globalOrderP = 0;
            ConjuntoTokens(ref listaTokens, ref globalOrderP);
            listaTokens.ForEach(x => Console.WriteLine(x.TClasificacion + ", " + x.TContenido + ", " + x.TGOrder + ", " + x.TControl + ", " + x.TSImpresion));
            match(Tipos.FinProduccion);
            // Necesito verificar si hay un or dentro de otro or, digo, si hay algo en la lista
            if(listaTokens.Count > 0)
            {
                for(int i = 1; i <= globalOrderP; i++)
                {
                    if(listaTokens.Exists(x => x.TGOrder == i))
                    {
                        if(listaTokens.Find(x => x.TGOrder == i)?.TControl == Control.Secuencial)
                        {
                            int firstOccurence, lastOccurence;
                            firstOccurence = listaTokens.FindIndex(x => x.TGOrder == i);
                            lastOccurence = listaTokens.FindLastIndex(x => x.TGOrder == i);
                            for(int j = firstOccurence; j <= lastOccurence; j++) if(listaTokens[j].TGOrder != i) throw new Error("NO PUEDE HABER NADA DENTRO DE UN OR, NI OTRO OR", log, linea);
                        }
                    }
                }
            }
            // Ahora con todas estas listas, toca trabajar en la impresion y debe ser simplista
            servicioDeImpresion(listaTokens);
            IndentCont--;
            lenguajecs.WriteLine(IndentString() + "}");
            if (Clasificacion == Tipos.SNT)
            {
                Producciones();
            }
        }
        private void ConjuntoTokens(ref List<TokensOpt> listaTokens, ref int control)
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
                // ! Necesito arreglar el orden, uno por parentesis para diferenciarlos
                // ! y el control se agregara adentro de la estructura en la lista, no un stack separado
                // ? Pasaria por referencia o como recursividad el contador global de parentesis?
                control++;
                conjuntoDelParentesis(ref listaTokens,ref control);
                // la impresion viene hasta el primer parentesis izquiero y despues
                // empieza la siguiente hasta el derecho, la parte inicial se separa del cuerpo
                // que dentro puede tener otro cuerpo
            }
            if (Clasificacion != Tipos.FinProduccion)
            {
                ConjuntoTokens(ref listaTokens,ref control);
            }
        }
        // Pasar la cola de tokens para imprimir en orden, tal vez use un "servicio de impresion"
        private void conjuntoDelParentesis(ref List<TokensOpt> listaTokens, ref int control)
        {
            int localControl = control;
            match(Tipos.PIzquierdo);
            log.WriteLine("------ Almacenaje de tokens en parentesis ------");
            while(Clasificacion != Tipos.PDerecho)
            {
                if(Clasificacion == Tipos.ST)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localControl));
                    match(Tipos.ST);
                }
                else if(Clasificacion == Tipos.SNT)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localControl));
                    match(Tipos.SNT);
                }
                else if(Clasificacion == Tipos.Tipo)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, Contenido, localControl));
                    match(Tipos.Tipo);
                }
                else if(Clasificacion == Tipos.Or)
                {
                    listaTokens.Add(new TokensOpt(Clasificacion, "", localControl));
                    match(Tipos.Or);
                }
                else if(Clasificacion == Tipos.PIzquierdo)
                {
                    // No permite un parentesis dentro de otro parentesis
                    // throw new Error("TIPO NO PERMITIDO", log, linea);
                    // Intentemos hacerlo
                    control++;
                    conjuntoDelParentesis(ref listaTokens, ref control);
                }
            }
            log.WriteLine("------ Fin Almacenaje de tokens en parentesis ------");
            match(Tipos.PDerecho);
            Console.WriteLine("El orden de este parentesis es: " + localControl);
            // Post almacenaje se hacen las comprobaciones y generacion de codigo
            // ! El problema del epsilon ahora es que depende de donde coloques sus partes no encapsula todo lo que
            // ! quisieras que encapsule, una solucion que veo es ver cuantos aumentos ha habido en el orden global a partir de donde se quedo
            // ! ademas de ver la distancia que tiene al token de control final de este EPSILON en este parentesis
            // ! si el token de CONTROL FINAL no es el ultimo guardado en este PARENTEISIS EPSILON entonces
            // ! Agrego un DUMMY extra para que sea el final, ahora la pregunta es cual sera el dummy?
            // ! Tengo que ver cuales tipos tengo a la mano, solo aplicable al caso epsilon, y si agrego el epsilon como dummy?
            // ! Me parece que esto lo hare con el orden global
            if(Clasificacion == Tipos.Epsilon)
            {
                // Si en el mismo orden hay suficientes ORS salta error
                if(listaTokens.FindAll(x => x.TGOrder == localControl).Any(x => x.TClasificacion == Tipos.Or)) throw new Error("No puede haber OR junto con EPSILON", log, linea);
                // Comprobar que haya suficientes cosas adentro
                if(listaTokens.FindAll(x => x.TGOrder == localControl).Count < 2) throw new Error ("No hay suficientes TIPOS para el EPSILON", log, linea);
                // Es obligatorio especificar la condicion de recurisivdad
                if(listaTokens.FindAll(x => x.TGOrder == localControl).First().TClasificacion == Tipos.SNT) throw new Error("ES NECESARIO ESPECIFICAR LA CONDICION DE RECURSIVIDAD ST O TIPOS", log, linea);
                // Encapsulacion adicional
                if(listaTokens.Last().TGOrder != localControl)
                {
                    listaTokens.Add(new TokensOpt(Tipos.Epsilon,"",localControl));
                }
                // Agrego el control
                listaTokens.FindAll(x => x.TGOrder == localControl).ForEach(x => x.TControl = Control.Recursivo);
                listaTokens.FindAll(x => x.TGOrder == localControl).First().TSImpresion = SecuenciaImpresion.Encabezado;
                listaTokens.FindAll(x => x.TGOrder == localControl).Last().TSImpresion = SecuenciaImpresion.Final;
                match(Tipos.Epsilon);
            }
            else
            {
                // Si en el mismo orden hay suficientes ORS
                if(listaTokens.FindAll(x => x.TGOrder == localControl).Any(x => x.TClasificacion == Tipos.Or))
                {
                    // Como veo si hay un or adentro de un OR
                    // Veo si estan en orden los ORS
                    for(int i = 0; i < listaTokens.FindAll(x => x.TGOrder == localControl).Count; i++) if(i % 2 != 0) if(listaTokens.FindAll(x => x.TGOrder == localControl).ElementAt(i).TClasificacion != Tipos.Or) throw new Error("El OR no esta en orden",log,linea);
                    // Veo si son ST o TIPO
                    for(int i = 0; i < (listaTokens.FindAll(x => x.TGOrder == localControl).Count - 1); i++) if(listaTokens.FindAll(x => x.TGOrder == localControl).ElementAt(i).TClasificacion == Tipos.SNT) throw new Error("NO PUDES USAR SNT EN LOS IF SECUENCIALES",log,linea);
                    // Elimino los OR, ahora no esta eliminando los ors, no se por que
                    Console.WriteLine(listaTokens.FindAll(x => x.TGOrder == localControl).RemoveAll(x => x.TClasificacion == Tipos.Or)); 
                    // Ahora lo hago imprmible como Condicionado y tambien añado el inicio y la cola
                    listaTokens.FindAll(x => x.TGOrder == localControl).ForEach(x => x.TControl = Control.Secuencial);
                    listaTokens.FindAll(x => x.TGOrder == localControl).First().TSImpresion = SecuenciaImpresion.Encabezado;
                    listaTokens.FindAll(x => x.TGOrder == localControl).Last().TSImpresion = SecuenciaImpresion.Final;
                }
                else 
                {
                    // Los hago a todos normales
                    listaTokens.FindAll(x => x.TGOrder == localControl).ForEach(x => x.TControl = Control.Normal);
                }
            }
        }
        // Para las condiciones solo se imprimen las "Tapas"
        private void servicioDeImpresion(List<TokensOpt> listaTokens)
        {
            foreach (TokensOpt t in listaTokens)
            {
                // Primero debo de ver que tipo de control es
                Console.WriteLine("Elemento en " + t.TGOrder);
                if(t.TControl == Control.Normal)
                {
                    if (t.TClasificacion == Tipos.SNT) lenguajecs.WriteLine(IndentString() + t.TContenido + "();");
                    else if (t.TClasificacion == Tipos.ST) lenguajecs.WriteLine(IndentString() + "match(\"" + t.TContenido + "\");");
                    else if (t.TClasificacion == Tipos.Tipo) lenguajecs.WriteLine(IndentString() + "match(Tipos." + t.TContenido + ");");
                }
                else if(t.TControl == Control.Recursivo)
                {
                    if(t.TSImpresion == SecuenciaImpresion.Encabezado)
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
                    else if(t.TSImpresion == SecuenciaImpresion.Final && t.TContenido != "")
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
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                    }
                    else if(t.TSImpresion == SecuenciaImpresion.Final && t.TContenido == "")
                    {
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
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
                else if (t.TControl == Control.Secuencial)
                {
                    if(t.TSImpresion == SecuenciaImpresion.Encabezado)
                    {
                        lenguajecs.Write(IndentString() + "if (");
                        if (t.TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine("Contenido == \"" + t.TContenido + "\")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(\"" + t.TContenido + "\");");
                        }
                        else if (t.TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine("Clasificacion == Tipos." + t.TContenido + ")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + t.TContenido + ");");
                        }
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                    }
                    else if(t.TSImpresion == SecuenciaImpresion.Final)
                    {
                        lenguajecs.WriteLine(IndentString() + "else");
                        lenguajecs.WriteLine(IndentString() + "{");
                        IndentCont++;
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
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                    }
                    else if(t.TSImpresion == SecuenciaImpresion.Cuerpo && t.TContenido != "")
                    {
                        lenguajecs.Write(IndentString() + "else if (");
                        if (t.TClasificacion == Tipos.ST)
                        {
                            lenguajecs.WriteLine("Contenido == \"" + t.TContenido + "\")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(\"" + t.TContenido + "\");");
                        }
                        else if (t.TClasificacion == Tipos.Tipo)
                        {
                            lenguajecs.WriteLine("Clasificacion == Tipos." + t.TContenido + ")");
                            lenguajecs.WriteLine(IndentString() + "{");
                            IndentCont++;
                            lenguajecs.WriteLine(IndentString() + "match(Tipos." + t.TContenido + ");");
                        }
                        IndentCont--;
                        lenguajecs.WriteLine(IndentString() + "}");
                    }
                }
            }
        }
        private class TokensOpt
        {
            public TokensOpt(Tipos _clasificacion, string _contenido = "", int _globalOrder = 0)
            {
                _TClasificacion = _clasificacion;
                _TContenido = _contenido;
                _TGOrder = _globalOrder;
                TSImpresion = SecuenciaImpresion.Cuerpo;
            }
            private int _TGOrder;
            private string _TContenido;
            private Tipos _TClasificacion;
            public SecuenciaImpresion TSImpresion { get; set; }
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
            public Control TControl { get; set; }
        }
        public enum Control
        {
            Recursivo, Secuencial, Normal
        }
        public enum SecuenciaImpresion
        {
            Encabezado, Cuerpo, Final
        }
    }
}