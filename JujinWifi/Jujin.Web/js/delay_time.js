$(function(){
	var leftSeconds = 100;//倒计时时间3秒
    	var intervalId;
	var $i= 0;
	intervalId = setInterval(function(){//倒计时方法
		if (leftSeconds <= 1) {
		    clearInterval(intervalId); //取消由 setInterval() 设置的 timeout 
			//跳转
		    //location.href="test.php";
		    return;  
		}  
		leftSeconds--;
		//$('.renwubuzhou p').eq($i).stop(true,true).fadeIn(1000);//每隔1秒显示一个元素
		$i++;
		if($i<=100){
			$(".progress_plus").html("<div class=\"bar\" style=\"width:"+$i+"%;\"></div>");
		}else{
			$(".progress_plus").html("<div class=\"bar\" style=\"width:100%;\"></div>");
		}

		$(".delay_time").html(Math.floor(leftSeconds/35)+1);	
	},28);//调用倒计时的方法  	
	
});
