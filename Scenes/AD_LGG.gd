extends Control


@onready var label = $TimeLeft
@onready var timer = $Timer

func _on_button_pressed() -> void:
	hide()

func find_time_left():
	var time_left = timer.time_left
	var second = int(time_left) % 60 + 1
	return [second]
	
func _process(delta):
	if timer.time_left <= 0:
		label.hide()
	else:
		label.text = "00:%02d" % find_time_left()
