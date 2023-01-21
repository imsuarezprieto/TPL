namespace TPL;


using System.Text.RegularExpressions;


public static class Program {

	private static readonly Processor processor = new Processor();

	public static async Task Main() {
		try {
			await Enumerable.Range( start: 0, count: 10 )
			                .Select( n => $"fila-{n:0000}.txt" )
			                .AggregateFor( fileName => processor.ProcessFile( fileName ) );
		}
		catch (AggregateException e) {
			Console.WriteLine( "OOPS" );
			throw;
		}
	}

}

public class Processor {

	private String file   = String.Empty;
	private Int16  number = 0;

	public async Task ProcessFile(String fileName) {
		this.file = fileName;
		this.number = Int16.Parse( Regex.Match( fileName, @"\d+" )
		                                .Value );
		await Task.Delay( 100 );
		if (number % 2 != 0) throw new Exception( $"{this.file}!" );
	}

}

public static class TPLX {

	public static async Task AggregateFor<T>(this IEnumerable<T> collection, Func<T, Task> function) {
		await Task.Run( () => {
			collection.AggregateSelect( function )
			          .Where( exception => exception is not null )
			          .AggregateException()
			         ?.Throw();
		} );
	}

	private static IEnumerable<Exception?> AggregateSelect<T>(this IEnumerable<T> collection, Func<T, Task> function) =>
			collection
				   .Select( element => 
							AggregateRun( () => function( element ) ) );

	private static Exception? AggregateRun(Func<Task> function) {
		try {
			function()
				   .GetAwaiter()
				   .GetResult();
			return default;
		}
		catch (Exception e) {
			return e;
		}
	}

	private static AggregateException? AggregateException(this IEnumerable<Exception?> exceptions) =>
			exceptions.Any() 
					? new AggregateException( exceptions ) 
					: default;

	private static void Throw(this AggregateException? exception) {
		if (exception is not null) throw exception;
	}

}
