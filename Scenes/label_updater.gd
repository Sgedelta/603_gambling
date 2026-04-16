extends Label

enum label_type {FLOAT, INT, PERCENT, STRING}

@export var prefix : String
@export var type : label_type
@export var float_precision : int = 2;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


func set_formatted_text(val):
	var formatStr : String
	match type:
		label_type.FLOAT:
			formatStr = "%.{prec}f".format({"prec" : float_precision}) % val
		label_type.INT:
			formatStr = "%d" % val
		label_type.PERCENT:
			formatStr = "%.{prec}f%%".format({"prec" : float_precision}) % (val*100)
		label_type.STRING:
			formatStr = val
		_:
			pass
	text = "{pre}{post}".format({"pre": prefix, "post" : formatStr})
