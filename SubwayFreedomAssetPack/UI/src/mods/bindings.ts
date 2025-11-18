import {bindValue, trigger} from "cs2/api";
import mod from "mod.json";

export const OnOpenPicker = () => trigger(mod.id, "OnOpenPicker");
