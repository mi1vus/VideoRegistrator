	/* MACROSCOP video settings */
	var serverUrl = "http://127.0.0.1:8080"
	var login = "root"
	var password = "";
	var channelnum = 0;
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
		partURL =  serverUrl + "/site?login="+ login + "&password=" + password +"&channelnum=" + channelnum + "&resolutionX=" + drawWidth + "&resolutionY=" + drawHeight;
		
		backImage.onload = showimage;
		backImage.src = partURL + "&id=" + randomString();
	} 