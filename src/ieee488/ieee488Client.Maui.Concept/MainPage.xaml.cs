using cc.isr.VXI11.IEEE488Client.Helper;

namespace cc.isr.VXI11.IEEE488Client.Maui.Concept;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		this.InitializeComponent();
	}

    private int _count = 0;
  
    private void OnCounterClicked( object sender, EventArgs e )
    {
        InstrumentId instrumentId = InstrumentId.K7510;
        Random rnd = new( DateTime.Now.Second );

        if ( this._count == 0 )
        {
            this.WelcomeLabel.Text = IEEE488InstrumentTestInfo.ResetClearDevice( instrumentId, TimeSpan.FromMilliseconds( 10 ) );
        }
        else
        {
            if ( rnd.NextDouble() > 0.5 )
                this.WelcomeLabel.Text = IEEE488InstrumentTestInfo.QueryIdentity( instrumentId, TimeSpan.FromMilliseconds( 10 ) );
            else
                this.WelcomeLabel.Text = IEEE488InstrumentTestInfo.QueryIdentity( instrumentId, TimeSpan.FromMilliseconds( 10 ) );
        }

        this._count++;
        this.CounterBtn.Text = $"Clicked {this._count} time{(this._count == 1 ? string.Empty : 's')}";
        this.InstrumentLabel.Text = $"{this._count} {IEEE488InstrumentTestInfo.QueryInfo}";

        SemanticScreenReader.Announce( this.CounterBtn.Text );
    }

}
