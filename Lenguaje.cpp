using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cplusplus
{
	public class Lenguaje : Sintaxis
	{
		public Lenguaje()
		{
		}
		public Lenguaje(string nombre) : base(nombre)
		{
		}
		public void Programa()
		{
			Librerias();
			Main();
		}
		public void Librerias()
		{
			match("#");
			match("include");
			match("<");
			match(Tipos.Identificador);
			match(">");
			if (Contenido == "?")
			{
				if (Contenido == "este")
				{
					match("este");
				}
				else
				{
					match("oeste");
				}
				match("condicionado");
			}
			if (Contenido == "#")
			{
				match("esto");
				if (Contenido == "&")
				{
					Hola();
				}
				match("es");
				match("recursivo");
				Si();
			}
		}
		public void Main()
		{
			match("void");
			match("main");
			match("(");
			match(")");
			BloqueInstrucciones();
		}
		public void BloqueInstrucciones()
		{
			if (Contenido == "listaInstrucciones")
			{
				match("listaInstrucciones");
			}
			else
			{
				Instruccion();
			}
		}
		public void ListaInstrucciones()
		{
			Instruccion();
			if (Contenido == ".")
			{
				Instruccion();
			}
		}
		public void Instruccion()
		{
		}
	}
}
