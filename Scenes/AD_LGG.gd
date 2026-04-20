extends CanvasLayer

signal ad_closed

@export var ad_videos: Array[VideoStream] = []
@onready var label = $Control/TimeLeft
@onready var timer = $Control/Timer
@onready var background = $Control
@onready var video_player = $Control/VideoStreamPlayer
var vidnum

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if ad_videos.size() > 0:
		vidnum = randi() % ad_videos.size()
		video_player.stream = ad_videos[vidnum]
		video_player.play()
	
func _on_button_pressed() -> void:
	ad_closed.emit()
	queue_free()

func find_time_left():
	var time_left = timer.time_left
	var second = int(time_left) % 60 + 1
	return [second]
	
func _process(delta):
	if timer.time_left <= 0:
		label.hide()
	else:
		label.text = "00:%02d" % find_time_left()
