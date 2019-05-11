using Irony.Parsing;

namespace BO_ZM_GSC_Injector {
    /// <summary>
    ///     The grammar of the GameScript language.
    /// </summary>
    [Language("GameScript", "1.0", "GameScript grammar for the Call of Duty: Black Ops")]
    public class GSCGrammar : Grammar {
        public GSCGrammar() {
            #region Lexical structure

            //Comments
            var blockComment = new CommentTerminal("block-comment", "/*", "*/");
            var lineComment = new CommentTerminal("line-comment", "//",
                "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(blockComment);
            NonGrammarTerminals.Add(lineComment);

            //Literals
            var numberLiteral = new NumberLiteral("numberLiteral", NumberOptions.AllowSign);
            var stringLiteral = new StringLiteral("stringLiteral", "\"");
            var identifier = new IdentifierTerminal("identifier", @"_/\", "_");

            MarkPunctuation("(", ")", "{", "}", "[", "]", ",", ".", ";", "::", "[[", "]]", "#include", "#using_animtree");

            RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/", "%");
            RegisterOperators(3, "|", "&", "^");
            RegisterOperators(4, "&&", "||");
            RegisterBracePair("(", ")");

            #endregion

            var program = new NonTerminal("program");
            var functions = new NonTerminal("functions");
            var function = new NonTerminal("function");
            var declarations = new NonTerminal("declarations");
            var declaration = new NonTerminal("declaration");

            var includes = new NonTerminal("includes");
            var include = new NonTerminal("include");
            var gscForFunction = new NonTerminal("gscForFunction");
            var baseCall = new NonTerminal("baseCall");
            var baseCallPointer = new NonTerminal("baseCallPointer");
            var scriptFunctionCall = new NonTerminal("scriptFunctionCall");
            var scriptFunctionCallPointer = new NonTerminal("scriptFunctionCallPointer");
            var scriptMethodCall = new NonTerminal("scriptMethodCall");
            var scriptMethodCallPointer = new NonTerminal("scriptMethodCallPointer");
            var scriptThreadCall = new NonTerminal("scriptThreadCall");
            var scriptThreadCallPointer = new NonTerminal("scriptThreadCallPointer");
            var scriptMethodThreadCall = new NonTerminal("scriptMethodThreadCall");
            var scriptMethodThreadCallPointer = new NonTerminal("scriptMethodThreadCallPointer");
            var call = new NonTerminal("call");
            var simpleCall = new NonTerminal("simpleCall");
            var parenParameters = new NonTerminal("parenParameters");
            var parameters = new NonTerminal("parameters");
            var expr = new NonTerminal("expr");
            var setVariableField = new NonTerminal("setVariableField");
            var array = new NonTerminal("array");
            var vector = new NonTerminal("vector");
            var _operator = new NonTerminal("operator");
            var relationalOperator = new NonTerminal("relationalOperator");
            var expression = new NonTerminal("expression");
            var relationalExpression = new NonTerminal("relationalExpression");
            var directAccess = new NonTerminal("directAccess");
            var boolNot = new NonTerminal("boolNot");
            var wait = new NonTerminal("wait");
            var size = new NonTerminal("size");
            var isString = new NonTerminal("isString");
            var hashedString = new NonTerminal("hashedString");

            var statement = new NonTerminal("statement");
            var ifStatement = new NonTerminal("ifStatement");
            var elseStatement = new NonTerminal("elseStatement");
            var whileStatement = new NonTerminal("whileStatement");
            var forStatement = new NonTerminal("forStatement");
            var forBody = new NonTerminal("forBody");
            var switchStatement = new NonTerminal("switchStatement");
            var switchLabel = new NonTerminal("switchLabel");
            var switchContents = new NonTerminal("switchContents");
            var switchContent = new NonTerminal("switchContent");
            var foreachStatement = new NonTerminal("foreachStatement");
            var booleanExpression = new NonTerminal("booleanExpression");
            var block = new NonTerminal("block");
            var blockContent = new NonTerminal("blockContent");
            var statementBlock = new NonTerminal("statementBlock");
            var shortExprOperator = new NonTerminal("shortExprOperator");
            var forIterate = new NonTerminal("forIterate");
            var conditionalStatement = new NonTerminal("conditionalStatement");
            var _return = new NonTerminal("return");
            var getFunction = new NonTerminal("getFunction");
            var developerScript = new NonTerminal("developerScript");
            var animTree = new NonTerminal("animTree");
            var usingAnimTree = new NonTerminal("usingAnimTree");
            var getAnimation = new NonTerminal("getAnimation");
            var waitTillFrameEnd = new NonTerminal("waitTillFrameEnd");
            var jumpStatement = new NonTerminal("jumpStatement");
            var parenExpr = new NonTerminal("parenExpr");

            waitTillFrameEnd.Rule = identifier + ";";
            usingAnimTree.Rule = ToTerm("#using_animtree") + "(" + stringLiteral + ")" + ";";
            getAnimation.Rule = ToTerm("%") + identifier;
            animTree.Rule = ToTerm("#animtree");
            program.Rule = includes + usingAnimTree + functions | usingAnimTree + functions | includes + functions | functions;
            functions.Rule = MakePlusRule(functions, function);
            function.Rule = identifier + parenParameters + block;
            declarations.Rule = MakePlusRule(declarations, declaration);
            declaration.Rule = simpleCall | statement | setVariableField | wait | _return | waitTillFrameEnd | jumpStatement;
            block.Rule = ToTerm("{") + blockContent + "}" | ToTerm("{") + "}";
            blockContent.Rule = declarations;
            parenExpr.Rule = ToTerm("(") + expr + ")";
            expr.Rule = directAccess | call | identifier | stringLiteral | array | numberLiteral | vector | expression |
                        relationalExpression | conditionalStatement | boolNot | size |
                        isString | getFunction | hashedString | getAnimation | animTree | parenExpr;
            parameters.Rule = MakeStarRule(parameters, ToTerm(","), expr) | expr;
            parenParameters.Rule = ToTerm("(") + parameters + ")" | "(" + ")";
            includes.Rule = MakePlusRule(includes, include);
            include.Rule = ToTerm("#include") + identifier + ";";

            array.Rule = expr + "[" + expr + "]" | ToTerm("[]");
            vector.Rule = ToTerm("(") + expr + "," + expr + "," + expr + ")";
            shortExprOperator.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "++" | "--";
            setVariableField.Rule = expr + shortExprOperator + expr + ";" | expr + shortExprOperator + ";";
            _operator.Rule = ToTerm("+") | "-" | "/" | "*" | "%" | "&" | "|";
            relationalOperator.Rule = ToTerm(">") | ">=" | "<" | "<=" | "==" | "!=";
            expression.Rule = expr + _operator + expr | "(" + expr + _operator + expr + ")";
            relationalExpression.Rule = expr + relationalOperator + expr;
            booleanExpression.Rule = expr + "&&" + expr
                                     | booleanExpression + "&&" + booleanExpression
                                     | ToTerm("(") + expr + "&&" + expr + ")"
                                     | expr + "||" + expr
                                     | ToTerm("(") + expr + "||" + expr + ")"
                                     | booleanExpression + "||" + booleanExpression
                                     | expr;

            directAccess.Rule = expr + "." + identifier;
            boolNot.Rule = ToTerm("!") + expr | ToTerm("!") + "(" + booleanExpression + ")";
            wait.Rule = ToTerm("wait") + expr + ";";
            size.Rule = expr + ".size";
            _return.Rule = ToTerm("return") + expr + ";" | ToTerm("return") + booleanExpression + ";" | ToTerm("return") + ";";
            jumpStatement.Rule = ToTerm("break") + ";" | ToTerm("continue") + ";";
            isString.Rule = ToTerm("&") + stringLiteral;
            hashedString.Rule = ToTerm("#") + stringLiteral;
            getFunction.Rule = ToTerm("::") + expr | gscForFunction + expr;

            gscForFunction.Rule = identifier + "::";
            baseCall.Rule = gscForFunction + identifier + parenParameters | identifier + parenParameters;
            baseCallPointer.Rule = ToTerm("[[") + expr + "]]" + parenParameters;
            scriptFunctionCall.Rule = baseCall;
            scriptFunctionCallPointer.Rule = baseCallPointer;
            scriptMethodCall.Rule = expr + baseCall;
            scriptMethodCallPointer.Rule = expr + baseCallPointer;
            scriptThreadCall.Rule = ToTerm("thread") + baseCall;
            scriptThreadCallPointer.Rule = ToTerm("thread") + baseCallPointer;
            scriptMethodThreadCall.Rule = expr + "thread" + baseCall;
            scriptMethodThreadCallPointer.Rule = expr + "thread" + baseCallPointer;

            call.Rule = scriptFunctionCall | scriptFunctionCallPointer | scriptMethodCall | scriptMethodCallPointer |
                        scriptThreadCall | scriptThreadCallPointer | scriptMethodThreadCall |
                        scriptMethodThreadCallPointer;
            simpleCall.Rule = call + ";";

            statementBlock.Rule = block | declaration;
            statement.Rule = ifStatement | whileStatement | forStatement | switchStatement | foreachStatement | developerScript;
            ifStatement.Rule = ToTerm("if") + "(" + booleanExpression + ")" + statementBlock |
                               ToTerm("if") + "(" + booleanExpression + ")" + statementBlock + elseStatement;
            elseStatement.Rule = ToTerm("else") + statementBlock | ToTerm("else") + ifStatement;
            whileStatement.Rule = ToTerm("while") + "(" + booleanExpression + ")" + statementBlock;
            forIterate.Rule = expr + shortExprOperator + expr | expr + shortExprOperator;
            forBody.Rule = setVariableField + booleanExpression + ";" + forIterate
                           | ToTerm(";") + booleanExpression + ";" + forIterate
                           | ToTerm(";") + ";" + forIterate
                           | ToTerm(";") + ";"
                           | setVariableField + ";" + forIterate
                           | setVariableField + ";"
                           | ToTerm(";") + booleanExpression + ";"
                           | setVariableField + booleanExpression + ";";

            forStatement.Rule = ToTerm("for") + "(" + forBody + ")" + statementBlock;
            foreachStatement.Rule = ToTerm("foreach") + "(" + identifier + "in" + expr + ")" + statementBlock;
            switchLabel.Rule = ToTerm("case") + expr + ":" | ToTerm("default") + ":";
            switchContents.Rule = MakePlusRule(switchContents, switchContent);
            switchContent.Rule = switchLabel + blockContent | switchLabel;
            switchStatement.Rule = ToTerm("switch") + parenExpr + "{" + switchContents + "}";
            conditionalStatement.Rule = expr + "?" + expr + ":" + expr;
            developerScript.Rule = ToTerm("/#") + blockContent + "#/";
            Root = program;
        }

        public static string CheckSyntax(string fileText) {
            Grammar grammar = new GSCGrammar();
            Parser parser = new Parser(grammar);
            ParseTree parseTree = parser.Parse(fileText);

            if(parseTree.ParserMessages.Count < 1)
                return "";

            return parseTree.ParserMessages[0].Location.Line.ToString();
        }
    }
}
