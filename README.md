## Here are the steps for getting Minuet up and running

* Clone the repository

      <code>
      git clone git@github.com:SymphonyOSF/SFE-Minuet-DesktopClient.git
      </code>

* Download Express 2013 for Windows Desktop 

      http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx

* Open Symphony.sln in Visual Studio

* Change URL to point to the relevant URL

      In config.json,
      
      <code>
      "url" : "[your url]"
      </code>

* Open Paragon.sln in Visual Studio 

* Change Command line arguments to correct location

      Runtime >> Paragon >> Right-click >> Open properties >> Debug >> Command line arguments:
      
      <code>
      /start-app="[location to Symphony pgx]\Symphony.pgx" /env=development
      </code>
      
* This should fire-up the wrapper with Symphony running 
