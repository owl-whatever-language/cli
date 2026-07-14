namespace OwlDomain.ParsingTools.Diagnostics;

public sealed class DiagnosticSourceDisplay
{
	#region Nested types
	[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
	private sealed class LineInfo(TextFragmentLine text, int line)
	{
		#region Properties
		public TextFragmentLine Text { get; } = text;
		public int Line { get; } = line;
		public bool IsRelevant { get; set; } = false;
		public bool ShouldDim { get; set; } = true;
		public List<IDiagnostic> Diagnostics { get; } = [];
		public List<IDiagnosticAnnotation> Annotations { get; } = [];
		#endregion

		#region Helpers
		private string DebuggerDisplay() => $"Line #{Line} {{ Diagnostics = ({Diagnostics.Count}), Annotations = ({Annotations.Count}) }}";
		#endregion
	}
	private readonly record struct IrrelevantGroup(int Start, int End)
	{
		#region Properties
		public int Count => End - Start + 1;
		#endregion
	}
	#endregion

	#region Fields
	private readonly IClassificationStyling _styling;
	private readonly Dictionary<int, LineInfo> _lines = [];
	private readonly List<IDiagnostic> _sourceDiagnostics = [];
	private readonly TextFragmentLineCollection _output = [];
	#endregion

	#region Properties
	public ISourceFile Source { get; }
	#endregion

	#region Constructors
	public DiagnosticSourceDisplay(ISyntaxTree tree, IClassificationStyling styling)
	{
		_styling = styling;
		Source = tree.Source;

		TextFragmentLineCollection lines = tree.GetLines();

		foreach (TextFragmentLine line in lines)
		{
			Debug.Assert(line.Line is not null);
			_lines.Add(line.Line.Value, new(line, line.Line.Value));

			_output.Add(line);
		}
	}
	#endregion

	#region Mark methods
	public DiagnosticSourceDisplay MarkRelevant(int line, bool shouldDim)
	{
		if (_lines.TryGetValue(line, out LineInfo? info))
		{
			info.IsRelevant = true;

			if (info.ShouldDim is true)
				info.ShouldDim = shouldDim;
		}

		return this;
	}
	public DiagnosticSourceDisplay MarkRelevant(IEnumerable<int> lines, bool shouldDim)
	{
		foreach (int line in lines)
			MarkRelevant(line, shouldDim);

		return this;
	}
	public DiagnosticSourceDisplay MarkRelevant(PositionRange position, bool shouldDim)
	{
		int start = position.Start.Line;
		int end = position.End.Line;

		IEnumerable<int> lines = Enumerable.Range(start, end - start + 1);
		return MarkRelevant(lines, shouldDim);
	}
	public DiagnosticSourceDisplay MarkRelevant(ISyntaxNode node, bool shouldDim) => MarkRelevant(node.Position.WithoutIndex, shouldDim: shouldDim);
	#endregion

	#region Add methods
	public DiagnosticSourceDisplay Add(IDiagnostic diagnostic)
	{
		if (diagnostic.Source != Source)
			ThrowHelper.ThrowArgumentException(nameof(diagnostic), "The given diagnostic wasn't relevant to the current source file.");

		if (diagnostic.Position == default)
			_sourceDiagnostics.Add(diagnostic);
		else
		{
			int line = diagnostic.Position.Start.Line;
			if (_lines.TryGetValue(line, out LineInfo? info))
				info.Diagnostics.Add(diagnostic);

			MarkRelevant(diagnostic.Position, shouldDim: false);
		}

		// Note(Nightowl): Don't include the primary diagnostic message here as it makes it more confusing later;
		foreach (IDiagnosticAnnotation annotation in diagnostic.Annotations.Skip(1))
		{
			if (annotation.Source != Source)
				continue;

			if (annotation.Position == default)
				continue;

			if (_lines.TryGetValue(annotation.Position.Start.Line, out LineInfo? info))
				info.Annotations.Add(annotation);

			MarkRelevant(annotation.Position, shouldDim: false);
		}

		return this;
	}
	#endregion

	#region Apply methods
	public void ApplyTypicalTransformations()
	{
		ApplyDim();
		TrimIrrelevant();
		SnipIrrelevantGroups();
		TrimEndComments();
		//AttachAnnotations();
		//AttachDiagnostics();
		AttachLineAnnotations();
		AttachSourceDiagnostics();
	}
	public void ApplyDim()
	{
		foreach (LineInfo info in GetOrdered())
		{
			if (info.ShouldDim)
				info.Text.AddClassification(ClassificationKind.Dim);
		}

		foreach (TextFragmentLine line in _output)
		{
			// Todo(Nightowl): Only just noticed this again, does this actually need to be .Dim or should it be .Snipped?

			if (line.IsSnipped)
				line.AddClassification(ClassificationKind.Dim);
		}

		foreach (TextFragmentLine line in _output)
		{
			if (line.Line is not null)
				continue;

			line.Replace(f =>
			{
				if (f.Classifications.Any(c => c == ClassificationKind.Margin))
					return f.With(ClassificationKind.Dim);

				return f;
			});
		}
	}
	public TextFragmentLineCollection GetLines() => _output;
	#endregion

	#region Trim methods
	public void SnipIrrelevantGroups()
	{
		TextFragment snipFragment = GetSnipFragment();
		IReadOnlyList<IrrelevantGroup> irrelevantGroups = GetIrrelevantGroups();

		foreach (IrrelevantGroup group in irrelevantGroups)
		{
			Debug.Assert(group.Count > 0);

			if (group.Count is 1)
			{
				if (_output.TryGetLineAt(group.Start, out TextFragmentLine? line))
					line.AddClassification(ClassificationKind.Dim);
			}
			else
			{
				int startIndex = _output.FindIndex(l => l.Line == group.Start);
				if (startIndex < 0)
					continue; // Note(Nightowl): First / last lines that got trimmed, no snip needed here;

				for (int i = group.Start; i <= group.End; i++)
				{
					TextFragmentLine? toRemove = _output.TryGetLineAt(i);
					if (toRemove is not null)
						_output.Remove(toRemove);
				}

				TextFragmentLine? next = _output.TryGetLineAt(group.End + 1);
				TextFragmentLine? previous = _output.TryGetLineAt(group.Start - 1);
				TextFragment? indent = next?.Indentation ?? previous?.Indentation;

				TextFragmentLine snipLine = new(null, snipFragment);
				if (indent is not null)
					snipLine.Insert(0, indent.Value);

				_output.Insert(startIndex, snipLine);
			}
		}
	}
	public void TrimIrrelevant()
	{
		TrimEnd();
		TrimStart();
	}
	public void TrimStart()
	{
		foreach (LineInfo line in GetOrdered())
		{
			if (line.IsRelevant)
				break;

			_output.Remove(line.Text);
		}
	}
	public void TrimEnd()
	{
		foreach (LineInfo line in GetOrdered().Reverse())
		{
			if (line.IsRelevant)
				break;

			_output.Remove(line.Text);
		}
	}
	public void TrimEndComments() => _output.TrimEndComments();
	#endregion

	#region Attach methods
	public void AttachLineAnnotations()
	{
		foreach (LineInfo info in _lines.Values)
		{
			if (info.Diagnostics.Count is 0 && info.Annotations.Count is 0)
				continue;

			AttachLineAnnotations(info);
		}
	}
	private void AttachLineAnnotations(LineInfo info)
	{
		TextFragmentLine? target = _output.TryGetLineAt(info.Line);
		Debug.Assert(target is not null);

		int diagnosticCount = info.Diagnostics.Count;
		int annotationCount = info.Annotations.Count;
		Debug.Assert(diagnosticCount > 0 || annotationCount > 0);

		if (diagnosticCount is 1 && annotationCount is 0 && info.Diagnostics[0].IsPositionSpecific is false)
		{
			IDiagnostic diagnostic = info.Diagnostics[0];
			TextFragmentLine line = PrepareShortLine(diagnostic, true, null);
			target.AddRange(line);

			return;
		}

		if (annotationCount is 1 && diagnosticCount is 0 && info.Annotations[0].IsPositionSpecific is false)
		{
			IDiagnosticAnnotation annotation = info.Annotations[0];
			TextFragmentLine line = PrepareAnnotationMessage(annotation, true, null);
			target.AddRange(line);

			return;
		}

		if (diagnosticCount is 1 && info.Diagnostics[0].IsPositionSpecific is false)
		{
			IDiagnostic diagnostic = info.Diagnostics[0];
			TextFragmentLine line = PrepareShortLine(diagnostic, true, null);
			target.AddRange(line);
		}

		Dictionary<IDiagnosticAnnotation, TextFragmentLineCollection> messages = GetLineMessages(info);
		IDiagnosticAnnotation[] order = messages.Keys.OrderByDescending(a => a.Position.Start).ToArray();

		Dictionary<IDiagnosticAnnotation, TextFragmentCollection> prefixes = [];

		foreach (IDiagnosticAnnotation annotation in order)
			prefixes.Add(annotation, []);

		int column = 1;

		TextFragment? indent = target.Indentation;
		if (indent is not null)
		{
			foreach (TextFragmentCollection prefix in prefixes.Values)
				prefix.Add(indent.Value);

			column += indent.Value.Text.Length;
		}

		for (int i = order.Length - 1; i >= 0; i--)
		{
			IDiagnosticAnnotation annotation = order[i];

			ClassificationKind? classification = annotation.Message.SelectMany(l => l).Select(f => f.Classification).FirstOrDefault();
			classification ??= ClassificationKind.Message;

			Debug.Assert(annotation.Position.IsMultiline is false);

			int toStart = annotation.Position.Start.Column - column;
			int mid = (annotation.Position.End.Column - annotation.Position.Start.Column) / 2;
			toStart += mid;

			TextFragment space = new(" ", ClassificationKind.Whitespace);
			TextFragment alignment = new(new(' ', toStart), ClassificationKind.Whitespace);
			TextFragment underline = new("└", classification);
			TextFragment pipe = new("│", classification);
			column = annotation.Position.Start.Column + 1;

			prefixes[annotation].AddRange(alignment, underline, space);

			for (int j = i - 1; j >= 0; j--)
				prefixes[order[j]].AddRange(alignment, pipe);
		}

		foreach (var pair in prefixes)
			messages[pair.Key].Prefix(p => pair.Value);

		int targetIndex = _output.IndexOf(target);

		if (targetIndex < _output.Count - 1)
		{
			int nextLineLength = _output[targetIndex + 1].ToPlainText().Length;
			int annotationPosition = order.Last().Position.Start.Column;

			if (annotationPosition < nextLineLength)
				_output.InsertAfterLine(info.Line, new(null));
		}

		foreach (IDiagnosticAnnotation annotation in order.Reverse())
		{
			TextFragmentLineCollection message = messages[annotation];
			_output.InsertRangeAfterLine(info.Line, message);
		}

		TextFragmentLine overline = GetOverline(info, target);
		_output.InsertAfterLine(info.Line, overline);
	}
	private Dictionary<IDiagnosticAnnotation, TextFragmentLineCollection> GetLineMessages(LineInfo info)
	{
		Dictionary<IDiagnosticAnnotation, TextFragmentLineCollection> messages = [];

		foreach (IDiagnostic diagnostic in info.Diagnostics)
		{
			TextFragmentLineCollection message = PrepareMessage(diagnostic.Annotations[0], diagnostic.Kind);
			messages.Add(diagnostic.Annotations[0], message);
		}

		foreach (IDiagnosticAnnotation annotation in info.Annotations)
		{
			if (messages.ContainsKey(annotation))
				continue;

			TextFragmentLineCollection message = PrepareMessage(annotation, null);
			messages.Add(annotation, message);
		}

		return messages;
	}
	private TextFragmentLine GetOverline(LineInfo info, TextFragmentLine target)
	{
		PositionRange[] diagnosticPositions = info.Diagnostics
			.Where(d => d.IsPositionSpecific)
			.Select(d => d.Position)
			.OrderBy(d => d.Start)
			.ToArray();

		PositionRange[] annotationPositions = info.Annotations
			.Where(d => d.IsPositionSpecific)
			.Select(d => d.Position)
			.OrderBy(d => d.Start)
			.ToArray();

		PositionRange[] allPositions = diagnosticPositions
			.Concat(annotationPositions)
			.OrderBy(d => d.Start)
			.ToArray();

		List<TextFragment> fragments = [];

		int column = 1;

		if (target.Indentation is not null)
		{
			fragments.Add(target.Indentation.Value);
			column += target.Indentation.Value.Text.Length;
		}

		foreach (PositionRange current in allPositions)
		{
			Debug.Assert(current.IsMultiline is false);

			if (column < current.Start.Column)
			{
				TextFragment spacer = new(new(' ', current.Start.Column - column), ClassificationKind.Whitespace);
				fragments.Add(spacer);
				column = current.Start.Column;
			}

			IDiagnostic? diagnostic = info.Diagnostics.FirstOrDefault(d => d.Position == current);
			ClassificationKind classification = diagnostic?.Kind.ToClassification() ?? ClassificationKind.Message;

			string character = "─"; //diagnostic is not null ? Wavy : Underline;
			Debug.Assert(character.Length is 1);

			int length = Math.Max(1, current.End.Column - current.Start.Column);
			char[] text = new char[length];
			for (int i = 0; i < length; i++)
				text[i] = character[0];

			text[length / 2] = '┬';

			column += text.Length;

			TextFragment fragment = new(new(text), classification);
			fragments.Add(fragment);
		}

		return new(null, fragments);
	}
	public void AttachDiagnostics()
	{
		foreach (LineInfo info in _lines.Values)
		{
			if (info.Diagnostics.Count is 0)
				continue;

			TextFragmentLine? target = _output.TryGetLineAt(info.Line);
			Debug.Assert(target is not null);

			if (info.Diagnostics.Count is 1 && info.Annotations.Count is 0)
			{
				IDiagnostic diagnostic = info.Diagnostics[0];
				TextFragmentLine line = PrepareShortLine(diagnostic, true, null);
				target.AddRange(line);
			}
			else
			{
				TextFragment? indent = target.Indentation;

				List<TextFragmentLine> lines = [];
				foreach (IDiagnostic diagnostic in info.Diagnostics)
				{
					TextFragmentLine line = PrepareShortLine(diagnostic, false, indent);
					lines.Add(line);
				}

				_output.InsertRangeAfterLine(info.Line, lines);
			}
		}
	}
	public void AttachAnnotations()
	{
		foreach (LineInfo info in _lines.Values)
		{
			if (info.Annotations.Count is 0)
				continue;

			TextFragmentLine? target = _output.TryGetLineAt(info.Line);
			Debug.Assert(target is not null);

			if (info.Annotations.Count is 1 && info.Diagnostics.Count is 0)
			{
				IDiagnosticAnnotation annotation = info.Annotations[0];
				TextFragmentLine line = PrepareAnnotationMessage(annotation, true, null);
				target.AddRange(line);
			}
			else
			{
				TextFragment? indent = target.Indentation;

				List<TextFragmentLine> lines = [];
				foreach (IDiagnosticAnnotation annotation in info.Annotations)
				{
					TextFragmentLine line = PrepareAnnotationMessage(annotation, false, indent);
					lines.Add(line);
				}

				_output.InsertRangeAfterLine(info.Line, lines);
			}
		}
	}
	public void AttachSourceDiagnostics()
	{
		bool addedEmpty = false;
		foreach (IDiagnostic diagnostic in _sourceDiagnostics)
		{
			if (addedEmpty is false)
			{
				_output.Add(new(null));
				addedEmpty = true;
			}

			TextFragmentLine line = PrepareShortLine(diagnostic, false, null);
			_output.Add(line);
		}
	}
	#endregion

	#region Helpers
	private TextFragmentLineCollection PrepareMessage(IDiagnosticAnnotation annotation, DiagnosticKind? kind)
	{
		string symbolText =
			(kind is null ? null : _styling.GetSymbol(kind.Value)) ??
			_styling.GetSymbol(ClassificationKind.Diagnostic) ??
			_styling.GetSymbol(ClassificationKind.Message) ?? "⯌";

		TextFragment symbol = kind is null
			? new($" {symbolText}", null, [ClassificationKind.Diagnostic, ClassificationKind.Message])
			: new($" {symbolText}", null, kind.Value.ToClassification());

		TextFragmentLineCollection lines = [];

		foreach (ITextFragmentLine line in annotation.Message)
		{
			Debug.Assert(line.Line is null);

			TextFragmentLine newLine = new(null);
			newLine.AddRange(line);
			newLine.Add(symbol);

			lines.Add(newLine);
		}

		return lines;
	}
	private TextFragmentLine PrepareShortLine(IDiagnostic diagnostic, bool isSuffix, TextFragment? indent)
	{
		TextFragment prefix = GetDiagnosticFragment(diagnostic.Kind);

		TextFragmentLine line = new(null, diagnostic.ShortMessage);
		line.Insert(0, prefix);

		if (isSuffix)
			line.Insert(0, new(" ", ClassificationKind.Whitespace));
		else if (indent is not null)
			line.Insert(0, indent.Value);

		return line;
	}
	private TextFragmentLine PrepareAnnotationMessage(IDiagnosticAnnotation annotation, bool isSuffix, TextFragment? indent)
	{
		Debug.Assert(annotation.Message.Any());

		TextFragment prefix = GetAnnotationFragment();
		TextFragmentLine line = new(null, annotation.Message[0]);
		line.Insert(0, prefix);

		if (isSuffix)
			line.Insert(0, new(" ", ClassificationKind.Whitespace));
		else if (indent is not null)
			line.Insert(0, indent.Value);

		return line;
	}
	private TextFragment GetSnipFragment()
	{
		string symbol =
			_styling.GetSymbol(ClassificationKind.Snipped) ??
			_styling.GetSymbol(ClassificationKind.Message) ??
			_styling.GetSymbol(ClassificationKind.Diagnostic) ?? "//";

		TextFragment fragment = new($"{symbol} omitted code", null, ClassificationKind.Snipped);

		return fragment;
	}
	private TextFragment GetDiagnosticFragment(DiagnosticKind kind)
	{
		string symbol =
			_styling.GetSymbol(kind) ??
			_styling.GetSymbol(ClassificationKind.Diagnostic) ??
			_styling.GetSymbol(ClassificationKind.Message) ?? "⯌";

		TextFragment fragment = new($"{symbol} ", null, kind.ToClassification());
		return fragment;
	}
	private TextFragment GetAnnotationFragment()
	{
		string symbol =
			_styling.GetSymbol(ClassificationKind.Diagnostic) ??
			_styling.GetSymbol(ClassificationKind.Message) ?? "/⯌";

		TextFragment fragment = new($"{symbol} ", null, [ClassificationKind.Diagnostic, ClassificationKind.Message]);
		return fragment;
	}
	private IEnumerable<LineInfo> GetOrdered() => _lines.OrderBy(l => l.Key).Select(p => p.Value);
	private IReadOnlyList<IrrelevantGroup> GetIrrelevantGroups()
	{
		List<IrrelevantGroup> groups = [];

		int? start = null;
		int? last = null;

		foreach (LineInfo info in GetOrdered())
		{
			if (info.IsRelevant)
			{
				if (start is null)
					continue;

				Debug.Assert(start is not null);
				Debug.Assert(last is not null);

				groups.Add(new(start.Value, last.Value));

				start = null;
				last = null;

				continue;
			}

			Debug.Assert(info.Text.Line is not null);

			start ??= info.Text.Line;
			last = info.Text.Line;
		}

		if (start is not null)
		{
			Debug.Assert(last is not null);
			groups.Add(new(start.Value, last.Value));
		}

		return groups;
	}
	#endregion
}
