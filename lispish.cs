using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LispishParser
{
    // Stores symbols as an ennumerable
	public enum Symbol {
		Program,
		SExpr,
		List,
		Seq,
		Atom,
		LITERAL,
		REAL,
		INT,
		STRING,
		ID,
		INVALID
	}
	
	// Creates the node object
    public class Node
    {
        // Declraes fields
		public readonly Symbol Token;
		public string Text;
		private List <Node> children;
		
		/**
			Creates the lexeme and root nodes of the program
		*/
		public Node(Symbol token, string text) {
			this.Token = token;
			this.Text = text;
			this.children = new List<Node>();
		}
		
		/**
			Adds a new node to the children of the selected node
		*/
		public void addChild(Node child) {
			this.children.Add(child);
		}
		
		/**
			Adds a list of nodes to the children
		*/
		public void addChildren(List<Node> children) {
			this.children = children;	
		}
		
		/**
			Returns the token of the node
		*/
		public LispishParser.Symbol token{ 
			get {
				return this.Token;
			}
		}

		/**
			Prints the nodes and its children with an offset
			@param Prefix string the ammount of space infront of the Node
		*/
		public void Print(string prefix = "")
        {
			// Prints the cvurrent node's information
			Console.WriteLine($"{prefix}{this.Token.ToString().PadRight(41-prefix.Length)}{this.Text, 0}");
			
			// Prints each child node with the offset
			foreach (Node n in this.children) {
				n.Print(prefix + "  ");
			}
        }
    }

	/**
		Assigns a token to each lexeme in the input string
		@param src string The input string to be tokenized
		@reutrn The list of tokens
	*/
    static public List<Node> Tokenize(String src)
    {
        // Creates a list of nodes to return
		List<Node> result = new List<Node>();
		
		// Creates regex patterns to test the string
		string litPattern = @"\G[\\(\\)]";
		string realPattern = @"\G[+-]?[0-9]*\.[0-9]+";
		string intPattern = @"\G[+-]?[0-9]+";
		string stringPattern = @"\G""(?>\\""|.)*""";
		string idPattern = @"\G[^\s""\(\)]+";
		
		// Sets the starting index of the input string
		int index = 0;
		
		// Loops while contents are still inbounds
		while (index < src.Length) {
			// Checks which of the patterns match then adds to the result list and iterates the index
			if (Regex.Match(src.Substring(index), litPattern).Success) {
				Match m = Regex.Match(src.Substring(index), litPattern);
				result.Add(new Node(Symbol.LITERAL, m.Groups[0].Value));

				index+=m.Length;
			} else if (Regex.Match(src.Substring(index), realPattern).Success){
				Match m = Regex.Match(src.Substring(index), realPattern);
				result.Add(new Node(Symbol.REAL, m.Groups[0].Value));

				index+=m.Length;
			} else if (Regex.Match(src.Substring(index), intPattern).Success) {
				Match m = Regex.Match(src.Substring(index), intPattern);
				result.Add(new Node(Symbol.INT, m.Groups[0].Value));

				index+=m.Length;
			} else if (Regex.Match(src.Substring(index), stringPattern).Success) {
				Match m = Regex.Match(src.Substring(index), stringPattern);
				result.Add(new Node(Symbol.STRING, m.Groups[0].Value));

				index+=m.Length;
			} else if (Regex.Match(src.Substring(index), idPattern).Success) {
				Match m = Regex.Match(src.Substring(index), idPattern);
				result.Add(new Node(Symbol.ID, m.Groups[0].Value));
				
				index+=m.Length;
			} else if (src[index] == ' ' || src[index] == '\n') {
				index+=1;
			} else {
				throw new Exception();
			}
		}
		
		return result;
    }
	
	public class Parser {
		// Declare fields
		Node[] tokens;
		Node token;
		int index;
		Node programNode;
		
		/**
			Creates an object to parse a list of ordered tokens
			@param toks Node[] The list of tokens
		*/
		public Parser(Node[] toks) {
			this.tokens = toks;
			this.index = 0;
			this.token = this.tokens[index];
			this.programNode = program();
		}
		
		/**
			Returns the progam nod of the program
			@return The program node
		*/
		public Node getProgramNode() {
			return this.programNode;
		}
		
		public Node nextToken() {
			// Stores the old current token
			Node tok = this.token;
			
			// Iterates the index of the array
			this.index++;
			
			// Checks if the index is in bounds token is null othewise
			if (index < this.tokens.Length) {
				this.token = this.tokens[index];
			} else {
				this.token = null;
			}
			
			// Return the old token
			return tok;
		}
		
		/**
			Parses the Program level in the grammer
			@return Node of Program level and children
		*/
		Node program() {
			// Creates a program level node
			Node result = new Node(Symbol.Program, "");
			
			// Loops while SExpr are still present
			while (this.token != null) {
				result.addChild(this.sExpr());
			}
			
			// Returns the result
			return result;
		}
		
		/**
			Parses the SExpr level in the grammer
			@return Node of SExpr level and children
		*/
		Node sExpr() {
			// Creates an SExxpr level node
			Node sExpr = new Node(Symbol.SExpr, "");
			
			// Adds the first paren or an atom
			if (this.token.Text.Equals("(")) {
				sExpr.addChild(this.list());
			} else {
				sExpr.addChild(this.atom());	
			}
		
			// Returns the result
			return sExpr;
		}
		
		/**
			Parses the List level in the grammer
			@return Node of List level and children
		*/
		Node list() {
			// Creates a node for the list level
			Node result = new Node(Symbol.List, "");

			// Adds the left paren to the children
			Node lparen = this.nextToken();
			result.addChild(lparen);
			
			// Adds the end paren or enters the Seq level
			if (this.token.Text.Equals(")")) {
				result.addChild(this.token);
			} else {
				// Adds the Seq node to the children
				Node seq = this.seq();
				result.addChild(seq);
				
				// Adds the end paren
				if (this.token.Text.Equals(")")) {
					result.addChild(this.token);
					this.nextToken();
				}
			}
			
			// Returns the node
			return result;
		}
		
		/**
			Parses the Seq level in the grammer
			@return Node of Seq level and children
		*/
		Node seq() {
			// Creates a seq node
			Node result = new Node(Symbol.Seq, "");
			
			// Adds an SExp and its children
			result.addChild(this.sExpr());
			
			// Closes the sequence if an end paren is present
			if (!this.token.Text.Equals(")")) {
				result.addChild(this.seq());
			}
			
			// Returns the result
			return result;
		}
		
		/**
			Parses the Atom level in the grammer
			@return Node of Atom level and children
		*/
		Node atom() {
			// Creates an atom node
			Node result = new Node(Symbol.Atom, "");
			
			// Assigns the symbol based on the type of the atom
			if (this.token.Token.Equals("ID")) {
				result.addChild(this.token);	
			} else if (this.token.Token.Equals("INT")){
				result.addChild(this.token);	
			} else if (this.token.Token.Equals("REAL")){
				result.addChild(this.token);	
			} else {
				result.addChild(this.token);
			}
			
			// Goes to the next token in the list
			this.nextToken();
			
			// Retuerns the atom node and children
			return result;
		}
	}

	/**
		Parses the program and returns the tree
		@return The root node of the tree
	*/
    static public Node Parse(Node[] tokens)
    {
		// Parses the program
		Node program = new Parser(tokens).getProgramNode();
		
		// Returns the proogram
		return program;
    }

	/**
		Validates the input string and
		prints the tokens and parse tree
	*/
    static private void CheckString(string lispcode)
    {
        try
        {
            // Prints the input header
			Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

			// Tokenizes the string
            Node[] tokens = Tokenize(lispcode).ToArray();

			// Prints the token header
            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));

			// Prints the tokens
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.Token, -20} : {node.Text}");
            }

            Console.WriteLine(new String('-', 50));

			// Creates the parse tree
            Node parseTree = Parse(tokens);

			// Prints the parse tree section
            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
        // CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\\"" "")");
        // CheckString(@"(define foo ""Say \\""Chease!\\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
    }
}

