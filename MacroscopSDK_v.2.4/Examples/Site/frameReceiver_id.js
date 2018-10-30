	/* MACROSCOP video settings */
	var serverUrl = "http://127.0.0.1:8080"
	var login = "root"
	var password = "";
	var channelid = "4e29a54b-0009-46eb-82fe-6c9049adc0db";
	var drawWidth = 577;
	var drawHeight = 432;
	
	var reload_timer = setInterval("loading()", delay);
    var delay = 33;
	var backImage = new Image();
	var partURL = "";

    function showimage() 
	{
		document.frontImage.src = backImage.src;

		setTimeout(loading, delay); 
    }
	
	function randomString() 
	{
		return '' + new Date().getTime();
	}
	
    function loading() 
	{
		clearInterval(reload_timer);
		backImage.src = partURL + "&id=" + randomString();
    }
			
	onload = function()
	{
		partURL =  serverUrl + "/site?login="+ login + "&password=" + password +"&channelid=" + channelid + "&resolutionX=" + drawWidth + "&resolutionY=" + drawHeight;
		
		backImage.onload = showimage;
		backImage.src = partURL + "&id=" + randomString();
	} 