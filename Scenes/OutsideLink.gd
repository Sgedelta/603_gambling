extends Button

@onready var canvasLayer = get_parent().get_parent().get_parent()

func _on_pressed() -> void:
	if(canvasLayer.vidnum == 0):
		OS.shell_open("https://na.finalfantasyxiv.com/")
	else:
		OS.shell_open("https://www.ncpgambling.org/help-treatment/#:~:text=Gambling%20Helpline%E2%84%A2-,The%20National%20Problem%20Gambling%20Helpline%E2%84%A2%20(1%2D800%2DMY,50%20states%20and%20U.S.%20territories.")
